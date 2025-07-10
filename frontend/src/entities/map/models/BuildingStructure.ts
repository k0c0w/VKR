import { isLineString, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { Polygon, LineString, Feature, FeatureCollection, Point, Position } from "geojson";
import { FeatureWithId } from "./common";
import * as turf from "@turf/turf";
import { GEOJSON_PRECISION, INTERSECTION_TOLERANCE } from "@app/config/constants";
import { BuildingGeometry } from "./Building";
import guid from "@shared/types/guid";



export enum RoomType {
  Audience = "аудитория",
  Hall = "коридор",
}

export type RoomMetaProperties = {
  id?: string;
  type: RoomType;
  name?: string;
  meaning: "Room";
};

export type WallGeometry = LineString;
export type WallMetaProperties = {
  meaning: "Wall";
};

export type WallId = guid;

export interface Wall extends FeatureWithId<WallId, WallGeometry, WallMetaProperties> {}

export type RoomGeometry = Polygon;

export type RoomId = number;
export interface Room extends FeatureWithId<RoomId, RoomGeometry, RoomMetaProperties> {
  id: RoomId;
}

export function isWall(feature: Feature): feature is Wall {
  return isLineString(feature) && feature.properties?.meaning === "Wall";
}

export function isRoom(feature: Feature): feature is Room {
  return isPolygon(feature) && feature.properties?.meaning === "Room";
}

export function doesWallIntersectOtherWalls(wall: Feature<WallGeometry>, all: Feature<WallGeometry>[]): boolean {
  for (const otherWall of all) {
    const skipSelf = wall.id === otherWall.id;
    if (!skipSelf && !isCorrectWallRelation(wall, otherWall)) {
      return true;
    }
  }
  return false;
}

export function isCorrectWallRelation(wall: Feature<WallGeometry>, otherWall: Feature<WallGeometry>): boolean {
  const intersections = turf.lineIntersect(wall, otherWall);
  if (intersections.features.length === 0) {
    return true;
  }
  return allIntersectionsAreLineVertexes(intersections, wall.geometry.coordinates) &&
         allIntersectionsAreLineVertexes(intersections, otherWall.geometry.coordinates);
}

export function doesRoomIntersectOtherRooms(room: Feature<RoomGeometry>, all: Feature<RoomGeometry>[]): boolean {
  for (const otherRoom of all) {
    const skipSelf = otherRoom.id === room.id;
    if (!skipSelf && !isCorrectRoomRelation(room, otherRoom)) {
      return true;
    }
  }
  return false;
}

export function isCorrectRoomRelation(room: Feature<RoomGeometry>, other: Feature<RoomGeometry>): boolean {
  const truncatedRoom = turf.truncate(room, { precision: GEOJSON_PRECISION }) as Feature<Polygon>;
  const truncatedOther = turf.truncate(other, { precision: GEOJSON_PRECISION }) as Feature<Polygon>;
  const featureCollection = turf.featureCollection([truncatedRoom, truncatedOther]) as FeatureCollection<Polygon>;
  const intersection = turf.intersect(featureCollection);
  if (intersection && intersection.geometry) {
    const area = turf.area(intersection);
    if (area > INTERSECTION_TOLERANCE * INTERSECTION_TOLERANCE) { // Significant overlap
      return false;
    }
  }
  return true; // No significant overlap
}

export function hasSelfIntersection(feature: Feature<WallGeometry | RoomGeometry>) {
  return turf.kinks(feature).features.length > 0;
}

function allIntersectionsAreLineVertexes(intersections: FeatureCollection<Point>, currentFeatureVertices: Position[]): boolean {
  for (const { geometry } of intersections.features) {
    const [intersectionLng, intersectionLat] = geometry.coordinates;
    const intersectionIsVertexOnLine = currentFeatureVertices.some(
      ([vertexLng, vertexLat]) =>
        Math.abs(vertexLng - intersectionLng) < Math.pow(10, -GEOJSON_PRECISION) &&
        Math.abs(vertexLat - intersectionLat) < Math.pow(10, -GEOJSON_PRECISION)
    );
    if (!intersectionIsVertexOnLine) {
      return false;
    }
  }
  return true;
}

export function isWallInBounds(wall: Feature<WallGeometry>, bounds: BuildingGeometry): boolean {
  for (let i = 0; i < wall.geometry.coordinates.length; i++) {
    const position = wall.geometry.coordinates[i] as Position;
    if (!turf.booleanPointInPolygon(turf.point(position), bounds)) {
      return false;
    }
  }
  const basePolyHasHoles = bounds.coordinates.length > 1;
  if (basePolyHasHoles) {
    for (let i = 1; i < bounds.coordinates.length; i++) {
      const holeCoordinates = bounds.coordinates[i];
      const intersection = turf.lineIntersect(wall, turf.polygon([holeCoordinates]));
      if (intersection.features.length >= 1) {
        return false;
      }
    }
  }
  return true;
}

export function isRoomInBounds(room: Feature<RoomGeometry>, buildingBounds: BuildingGeometry): boolean {
  const truncatedRoom = turf.truncate(room, { precision: GEOJSON_PRECISION }) as Feature<Polygon>;
  const truncatedBounds = turf.truncate(buildingBounds, { precision: GEOJSON_PRECISION });
  
  // Check if all room vertices are inside bounds or within tolerance of boundary
  const boundsLine = turf.polygonToLine(truncatedBounds) as Feature<LineString>;
  const roomCoords = truncatedRoom.geometry.coordinates[0] as Position[];
  return roomCoords.every(coord => {
    const point = turf.point(coord);
    const isInside = turf.booleanPointInPolygon(point, truncatedBounds);
    const distanceToEdge = turf.pointToLineDistance(point, boundsLine, { units: 'degrees' });
    return isInside || distanceToEdge < INTERSECTION_TOLERANCE;
  });
}

export function roomHasCompleteState(room: Room): boolean {
  if (hasSelfIntersection(room)) {
    return false;
  }

  const { name, id, type } = room.properties;

  if (!Object.values(RoomType).includes(type)) {
    return false;
  }

  if (!name || name?.trim() === "") {
    return false;
  }

  /*
  todo: architectural id
  if (id === undefined || id?.trim() === "") {
    return false;
  }
  */

  return true;
}