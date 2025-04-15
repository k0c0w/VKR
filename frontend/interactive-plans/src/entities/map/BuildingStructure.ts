import { isLineString, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { Polygon, LineString, Feature } from "geojson";
import { FeatureWithId } from "./common";

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

export type WallGoometry = LineString;
export type WallMetaProperties = {
    meaning: "Wall";
};

export interface Wall extends FeatureWithId<WallGoometry, WallMetaProperties> {}

export type RoomGeometry = Polygon;
export interface Room extends FeatureWithId<RoomGeometry, RoomMetaProperties> {}

export function isWall(feature: Feature): feature is Wall {
    return isLineString(feature) && feature.properties?.meaning === "Wall";
}

export function isRoom(feature: Feature): feature is Room {
    return isPolygon(feature) && feature.properties?.meaning === "Room";
}