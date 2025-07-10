import FullPageTint from "@shared/ui/FullPageTint";
import ImageOverlayController from "./ImageOverlayController";
import { CircularProgress } from "@mui/material";
import { useEffect, useState } from "react";
import { Feature, Polygon, Position } from "geojson";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import L, { LatLng } from "leaflet";
import { useImageProcessingPipeline } from "@features/ai";
import AlertDialog from "@shared/ui/AlertDialog";
import ErrorMessage from "@shared/ui/ErrorMessage";
import { Room, RoomType } from "@entities/map";
import * as turf from '@turf/turf';
import numeric from 'numeric';
import cv from '@techstark/opencv-js';
import { setRoomsOnLevel } from "./planEditorSlice";
import { useBuildingMap } from "@shared/map";
import { generateRandomNumberWhichDoesNotExistsIn } from "@shared/utils/random";
import { useMap } from "react-leaflet";
import { GEOJSON_PRECISION } from "@app/config/constants";

function computeProjectiveTransform(cornerPixels: { x: number; y: number }[], imgWidth: number, imgHeight: number)
    : (x: number, y: number) => { x: number; y: number } {
  const correctedCornerPixels = [
    cornerPixels[0], // Top-left
    cornerPixels[1], // Top-right
    cornerPixels[3], // Bottom-right
    cornerPixels[2], // Bottom-left
  ];

  const srcPoints: [number, number][] = [
    [correctedCornerPixels[0].x, correctedCornerPixels[0].y],
    [correctedCornerPixels[1].x, correctedCornerPixels[1].y],
    [correctedCornerPixels[2].x, correctedCornerPixels[2].y],
    [correctedCornerPixels[3].x, correctedCornerPixels[3].y],
  ];

  const dstPoints: [number, number][] = [
    [0, 0], // Top-left
    [imgWidth, 0], // Top-right
    [imgWidth, imgHeight], // Bottom-right
    [0, imgHeight], // Bottom-left
  ];

  const A: number[][] = [];
  for (let i = 0; i < 4; i++) {
    const [sx, sy] = srcPoints[i];
    const [dx, dy] = dstPoints[i];
    A.push([sx, sy, 1, 0, 0, 0, -dx * sx, -dx * sy]);
    A.push([0, 0, 0, sx, sy, 1, -dy * sx, -dy * sy]);
  }
  const b: number[] = dstPoints.flat();
  const transform = numeric.solve(A, b);

  return (x: number, y: number) => {
    const denom = transform[6] * x + transform[7] * y + 1;
    if (denom === 0) {
      return { x: 0, y: 0 };
    }
    const xImg = (transform[0] * x + transform[1] * y + transform[2]) / denom;
    const yImg = (transform[3] * x + transform[4] * y + transform[5]) / denom;
    return { x: xImg, y: yImg };
  };
}

function convertGeoJsonToImageCoords(
  geoJsonFeature: Feature<Polygon>,
  corners: LatLng[],
  img: HTMLImageElement,
  map: L.Map
): { x: number; y: number }[] {
  const coordinates = geoJsonFeature.geometry.coordinates[0];
  const imgWidth = img.width;
  const imgHeight = img.height;
  const cornerPixels = corners.map((corner) => map.latLngToContainerPoint(corner));
  const transform = computeProjectiveTransform(cornerPixels, imgWidth, imgHeight);

  return coordinates.map((coord) => {
    const latLng = L.latLng(coord[1], coord[0]);
    const pixelPoint = map.latLngToContainerPoint(latLng);
    return transform(pixelPoint.x, pixelPoint.y);
  });
}

function extractImageFragment(
  geoJsonFeature: Feature<Polygon>,
  imageOverlayLayer: L.DistortableImageOverlay,
  map: L.Map,
  complete: (blob: Blob | null) => void
) {
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');
  if (!ctx) {
    throw new Error('Null canvas context');
  }

  const img = imageOverlayLayer.getElement();
  if (!img) {
    throw new Error('No image was provided');
  }

  canvas.width = img.width;
  canvas.height = img.height;

  const corners = imageOverlayLayer.getCorners();
  const imagePoints = convertGeoJsonToImageCoords(geoJsonFeature, corners, img, map);

  ctx.save();
  ctx.beginPath();
  ctx.moveTo(imagePoints[0].x, imagePoints[0].y);
  for (let i = 1; i < imagePoints.length; i++) {
    ctx.lineTo(imagePoints[i].x, imagePoints[i].y);
  }
  ctx.closePath();
  ctx.clip();
  ctx.drawImage(img, 0, 0, img.width, img.height);
  ctx.restore();

  canvas.toBlob(complete, "image/png");
}

function decodeBase64Mask(base64Mask: string, width: number, height: number): number[][] {
  const bytes = Uint8Array.from(atob(base64Mask), c => c.charCodeAt(0));
  const mask: number[][] = Array.from({ length: height }, () => Array(width).fill(0));

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const bitIndex = y * width + x;
      const byteIndex = Math.floor(bitIndex / 8);
      const bitOffset = bitIndex % 8;
      if (byteIndex < bytes.length) {
        const bit = (bytes[byteIndex] >> (7 - bitOffset)) & 1;
        mask[y][x] = bit;
      }
    }
  }

  return mask;
}

function traceContours(mask: number[][]): [number, number][][] {
  const height = mask.length;
  const width = mask[0].length;

  const mat = cv.matFromArray(height, width, cv.CV_8U, mask.flat());

  const contours = new cv.MatVector();
  const hierarchy = new cv.Mat();
  cv.findContours(mat, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);

  const contourList: [number, number][][] = [];
  for (let i = 0; i < contours.size(); i++) {
    const contour = contours.get(i);
    const points: [number, number][] = [];
    for (let j = 0; j < contour.data32S.length; j += 2) {
      const x = contour.data32S[j];
      const y = contour.data32S[j + 1];
      points.push([x, y]);
    }
    if (points.length > 0) {
      contourList.push(points);
    }
    contour.delete();
  }

  mat.delete();
  contours.delete();
  hierarchy.delete();

  return contourList;
}

function computeInverseProjectiveTransform(cornerPixels: { x: number; y: number }[], imgWidth: number, imgHeight: number)
    : (x: number, y: number) => { x: number; y: number } {
  const correctedCornerPixels = [
    cornerPixels[0], // Top-left
    cornerPixels[1], // Top-right
    cornerPixels[3], // Bottom-right
    cornerPixels[2], // Bottom-left
  ];

  const srcPoints: [number, number][] = [
    [0, 0], // Top-left
    [imgWidth, 0], // Top-right
    [imgWidth, imgHeight], // Bottom-right
    [0, imgHeight], // Bottom-left
  ];

  const dstPoints: [number, number][] = [
    [correctedCornerPixels[0].x, correctedCornerPixels[0].y],
    [correctedCornerPixels[1].x, correctedCornerPixels[1].y],
    [correctedCornerPixels[2].x, correctedCornerPixels[2].y],
    [correctedCornerPixels[3].x, correctedCornerPixels[3].y],
  ];

  const A: number[][] = [];
  for (let i = 0; i < 4; i++) {
    const [sx, sy] = srcPoints[i];
    const [dx, dy] = dstPoints[i];
    A.push([sx, sy, 1, 0, 0, 0, -dx * sx, -dx * sy]);
    A.push([0, 0, 0, sx, sy, 1, -dy * sx, -dy * sy]);
  }
  const b: number[] = dstPoints.flat();
  const transform = numeric.solve(A, b);

  return (x: number, y: number) => {
    const denom = transform[6] * x + transform[7] * y + 1;
    if (denom === 0) {
      return { x: 0, y: 0 };
    }
    const xImg = (transform[0] * x + transform[1] * y + transform[2]) / denom;
    const yImg = (transform[3] * x + transform[4] * y + transform[5]) / denom;
    return { x: xImg, y: yImg };
  };
}

function simplifyPolygon(polygon: Polygon, baseTolerance: number): Polygon {
  const coords = polygon.coordinates[0];

  // Step 1: Filter collinear points
  const filteredCoords = filterCollinearPoints(coords, 0.05); // Angle tolerance of 0.05 radians (~2.9°)

  // Step 2: Create a new Polygon with filtered coordinates
  const filteredPolygon: Polygon = {
    type: 'Polygon',
    coordinates: [filteredCoords],
  };

  // Step 3: Apply turf.simplify with the fixed baseTolerance
  const simplified = turf.simplify(filteredPolygon, {
    tolerance: baseTolerance,
    highQuality: true,
    mutate: true,
  });

  return simplified;
}

function filterCollinearPoints(coords: Position[], angleTolerance: number): Position[] {
  const result: Position[] = [coords[0]];

  for (let i = 1; i < coords.length - 1; i++) {
    const prev = coords[i - 1];
    const curr = coords[i];
    const next = coords[i + 1];

    const v1x = prev[0] - curr[0];
    const v1y = prev[1] - curr[1];
    const v2x = next[0] - curr[0];
    const v2y = next[1] - curr[1];

    const mag1 = Math.sqrt(v1x * v1x + v1y * v1y);
    const mag2 = Math.sqrt(v2x * v2x + v2y * v2y);

    if (mag1 === 0 || mag2 === 0) continue;

    const dotProduct = v1x * v2x + v1y * v2y;
    const cosTheta = dotProduct / (mag1 * mag2);
    const angle = Math.acos(Math.max(-1, Math.min(1, cosTheta)));

    if (angle > angleTolerance) {
      result.push(curr);
    }
  }

  result.push(coords[coords.length - 1]);
  return result;
}

export default function AutomaticImageLabelingPlugin() {
  const [imageBlob, setImageBlob] = useState<Blob | null>(null);
  const { error: fetchPipeLineError, result } = useImageProcessingPipeline(imageBlob);
  const [error, setError] = useState<string | null>(null);
  const map = useMap();
  const { currentLevelIndex } = useBuildingMap();
  const building = useAppSelector((state) => state.planEditorSlice.building)!;
  const dispatch = useAppDispatch();
  const [imageInfo, setImageInfo] = useState<{ corners: L.LatLng[], width: number; height: number } | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  function analizeImageToPredictRooms(imageOverlay: L.DistortableImageOverlay) {
    try {
      if (!imageOverlay) {
        return;
      }
      setIsLoading(true);
      const img = imageOverlay.getElement();
      if (img === undefined) {
        return;
      }

      const complete = (blob: Blob | null) => {
        if (blob === null) {
          setImageBlob(null);
          setError("Не удалось обрезать изображение.");
        } else {
          setImageBlob(blob);
        }
      };
      extractImageFragment(building, imageOverlay, map, complete);

      setImageInfo({
        corners: imageOverlay.getCorners(),
        width: img.width,
        height: img.height,
      });
    } catch (error) {
      console.error('Error processing image:', error);
      setImageBlob(null);
      setIsLoading(false);
      setError("Не удалось обрезать изображение.");
    }
  }

  function completeLayout(potentialRooms: { text?: string; bbox: number[]; mask: string }[]) {
    if (!imageInfo) return;

    const cornerPixels = imageInfo.corners.map(corner => map.latLngToContainerPoint(corner));
    const inverseTransform = computeInverseProjectiveTransform(cornerPixels, imageInfo.width, imageInfo.height);

    const usedIds: number[] = [];
    const rooms: Room[] = potentialRooms.flatMap(room => {
      const [minX, minY, maxX, maxY] = room.bbox;
      const width = maxX - minX;
      const height = maxY - minY;

      const maskArray = decodeBase64Mask(room.mask, width, height);
      const contours = traceContours(maskArray);

      return contours.map(contour => {
        // Step 1: Convert contour to GeoJSON coordinates
        let coordinates: [number, number][] = contour.map(point => {
          const [x, y] = point;
          const imgX = x + minX;
          const imgY = y + minY;
          const { x: mapX, y: mapY } = inverseTransform(imgX, imgY);
          const latLng = map.containerPointToLatLng([mapX, mapY]);
          return [latLng.lng, latLng.lat];
        });

        // Close the polygon if needed
        const polygonIsClosed = coordinates.length > 0 && (
          coordinates[0][0] !== coordinates[coordinates.length - 1][0] ||
          coordinates[0][1] !== coordinates[coordinates.length - 1][1]
        );
        if (polygonIsClosed) {
          coordinates.push(coordinates[0]);
        }

        // Step 2: Create initial polygon
        let polygon: Polygon = {
          type: 'Polygon',
          coordinates: [coordinates],
        };

        // Step 3: Round coordinates to GeoJSON precision
        polygon = turf.truncate(polygon, { precision: GEOJSON_PRECISION, mutate: true }) as Polygon;

        // Step 4: Filter by area (>= 5 m²)
        const area = turf.area(polygon);
        if (area < 5) {
          return null; // Skip small polygons
        }

        // Step 5: Simplify polygons with >10 points to ~20 points
        if (polygon.coordinates[0].length > 10) {
          let simplifiedPolygon = simplifyPolygon(polygon, 0.00001); // Initial tolerance
          let pointCount = simplifiedPolygon.coordinates[0].length;

          // Adjust tolerance to target ~20 points
          let tolerance = 0.00001;
          const maxIterations = 10;
          let iteration = 0;

          while (pointCount > 20 && iteration < maxIterations) {
            tolerance *= 1.5; // Increase tolerance
            simplifiedPolygon = simplifyPolygon(polygon, tolerance);
            pointCount = simplifiedPolygon.coordinates[0].length;
            iteration++;
          }

          polygon = simplifiedPolygon;
        }

        // Step 6: Generate room
        const id = generateRandomNumberWhichDoesNotExistsIn(usedIds);
        usedIds.push(id);

        return {
          id,
          type: 'Feature' as const,
          geometry: polygon,
          properties: {
            type: RoomType.Audience,
            name: room.text, // room.text is string | undefined, matches RoomMetaProperties
            meaning: "Room"
          },
        } as Room; // Explicitly cast to Room
      }).filter((room): room is Room => room !== null); // Remove null rooms
    });

    dispatch(setRoomsOnLevel({ levelIndex: currentLevelIndex, rooms }));
  }

  useEffect(() => {
    if (fetchPipeLineError !== null) {
      setError(fetchPipeLineError);
      setIsLoading(false);
    }

    if (result !== null) {
      completeLayout(result);
      setIsLoading(false);
    }
  }, [fetchPipeLineError, result]);

  return (
    <>
      <ImageOverlayController onImageComputeClicked={analizeImageToPredictRooms} />
      <AlertDialog
        open={error !== null}
        handleClose={() => setError(null)}
        title="Ошибка"
      >
        <ErrorMessage>{error}</ErrorMessage>
      </AlertDialog>
      {isLoading && (
        <FullPageTint>
          <CircularProgress />
        </FullPageTint>
      )}
    </>
  );
}