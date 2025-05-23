import FullPageTint from "@shared/ui/FullPageTint";
import ImageOverlayController from "./ImageOverlayController";
import { CircularProgress } from "@mui/material";
import { useEffect, useState } from "react";
import numeric from 'numeric';
import { Feature, Polygon } from "geojson";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import L from "leaflet";
import { useMap } from "react-leaflet";
import { LatLng } from "leaflet";
import { aiApi } from "@features/ai";
import { ai } from "react-router/dist/development/route-data-CGHGzi13";
import AlertDialog from "@shared/ui/AlertDialog";
import ErrorMessage from "@shared/ui/ErrorMesage";

function computeProjectiveTransform(cornerPixels: { x: number; y: number }[], imgWidth: number, imgHeight: number)
    : (x: number, y: number) => { x: number; y: number } {
  // Correct the corner order: [top-left, top-right, bottom-right, bottom-left]
  const correctedCornerPixels = [
        // Top-left
        cornerPixels[0], 
        // Top-right
        cornerPixels[1],
        // Bottom-right
        cornerPixels[3], 
        // Bottom-left
        cornerPixels[2],  
  ];

  const srcPoints: [number, number][] = [
    // Top-left
    [correctedCornerPixels[0].x, correctedCornerPixels[0].y],
    // Top-right
    [correctedCornerPixels[1].x, correctedCornerPixels[1].y],
    // Bottom-right 
    [correctedCornerPixels[2].x, correctedCornerPixels[2].y],
    // Bottom-left 
    [correctedCornerPixels[3].x, correctedCornerPixels[3].y],
  ];

  const dstPoints: [number, number][] = [
    // Top-left
    [0, 0],
    // Top-right
    [imgWidth, 0],
    // Bottom-right
    [imgWidth, imgHeight],
    // Bottom-left
    [0, imgHeight], 
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
      // Fallback to avoid division by zero
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

  const imagePoints = coordinates.map((coord) => {
    const latLng = L.latLng(coord[1], coord[0]);
    const pixelPoint = map.latLngToContainerPoint(latLng);
    const imagePoint = transform(pixelPoint.x, pixelPoint.y);
    return imagePoint;
  });

  return imagePoints;
}

function extractImageFragment(
  geoJsonFeature: Feature<Polygon>,
  imageOverlayLayer: L.DistortableImageOverlay,
  map: L.Map
): string {
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

  const maskedImageDataUrl = canvas.toDataURL('image/png');
  return maskedImageDataUrl;
}

export default function AutomaticImageLabelingPlugin({ readonlyMode }: { readonlyMode: boolean }) {
  const [showLoader, setShowLoader] = useState(false);
  const map = useMap();
  const building = useAppSelector((state) => state.planEditorSlice.building)!;
  const [requestLabeling, {data:rLData, error:rLError, isSuccess:rLIsSuccess, isError:rLIsError}] = aiApi.useRequestPlanLabelingMutation();
  const [getLabelingResult, {data:gLData, error:gLError, isSuccess:gLIsSuccess, isError:gLIsError, reset:gLReset}] = aiApi.useLazyGetPlanLabelingResultQuery();
  const [error, setError] = useState<string | null>(null);
  const dispatch = useAppDispatch();

  useEffect(() => {
    if (gLIsSuccess && gLData) {
      //todo: set labels to plan
      setShowLoader(false);
    }

    if (gLIsError && gLError) {
      setShowLoader(false);
      setError("Ошибка");
    }
  }, [gLIsSuccess, gLIsError, gLData, gLError, setError, setShowLoader]);

  useEffect(() => {
    if (rLData && rLIsSuccess) {
      setTimeout(() => getLabelingResult({requestId: rLData.requiestId}), rLData.estimatedTime * 1000);
    }

    if (rLError && rLIsError) {
      setShowLoader(false);
      setError("Ошибка")
    }
  }, [rLData, rLError, rLIsSuccess, rLIsError, setShowLoader, setError, getLabelingResult]);

  function analizeImageToPredictRooms(imageOverlay: L.DistortableImageOverlay) {
    setShowLoader(true);
    try {
      const maskedImageUrl = extractImageFragment(building, imageOverlay, map);
      const base64Image = maskedImageUrl.replace('data:', '').replace(/^.+,/, '')
      requestLabeling({planImage: base64Image});
    } catch (error) {
      console.error('Error processing image:', error);
      alert('Failed to process image: ' + (error as Error).message);
      setShowLoader(false);
    }
  }

  return <>
      <ImageOverlayController readonlyMode={readonlyMode} onImageComputeClicked={analizeImageToPredictRooms} />
      <AlertDialog open={error !== null}
        handleClose={()=> setError(null)}
        title="Ошибка при обработке изображения">
          <ErrorMessage>
            {error}
          </ErrorMessage>
      </AlertDialog>
      {showLoader && (
        <FullPageTint>
          <CircularProgress />
        </FullPageTint>
      )}
   </>
}