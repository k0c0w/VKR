import { isLineString, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { Polygon, LineString, Feature, FeatureCollection, Point, Position } from "geojson";
import { FeatureWithId } from "./common";
import * as turf from "@turf/turf"
import { GEOJSON_VERTEX_COMPARISION_TOLERANCE } from "@app/config/constants";
import { BuildingGeometry } from "./Building";

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

export function doesWallIntersetOtherWalls(wall: FeatureWithId<WallGoometry>, all: FeatureWithId<WallGoometry>[]): boolean {
    for(const otherWall of all) {
        const skipSelf = wall.id === otherWall.id;
        if (!skipSelf && !isCorrectWallRelation(wall, otherWall)) {
            return true;
        } 
    }

    return false;
}

function isCorrectWallRelation(wall: Feature<WallGoometry>, otherWall: Feature<WallGoometry>): boolean {
    const intersections = turf.lineIntersect(wall, otherWall);
    return intersections.features.length === 0 || allIntersectionsAreLineVertexes(intersections, wall.geometry.coordinates);
}

export function doesRoomIntersectOtherRooms(room: FeatureWithId<RoomGeometry>, all: FeatureWithId<RoomGeometry>[]): boolean {
    for(const otherRoom of all) {
        const skipSelf = otherRoom.id === room.id;
        if (!skipSelf && !isCorrectRoomRelation(room, otherRoom)) {
            return true;
        }
    }

    return false;
}

function isCorrectRoomRelation(room: FeatureWithId<RoomGeometry>, other: FeatureWithId<RoomGeometry>): boolean {
    const targetLine = turf.polygonToLine(room) as Feature<LineString>;
    const levelLine = turf.polygonToLine(other)
    const intersections = turf.lineIntersect(targetLine, levelLine);
    const noOrValidIntersection = (intersections.features.length === 0 
            && !turf.booleanContains(room, other) 
            && !turf.booleanContains(other, room))
        || allIntersectionsAreLineVertexes(intersections, targetLine.geometry.coordinates);
    return noOrValidIntersection;
}

export function hasSelfIntersection(feature: Feature<WallGoometry | RoomGeometry>) {
    return turf.kinks(feature).features.length > 0;
}

function allIntersectionsAreLineVertexes(intersections:  FeatureCollection<Point>, currentFeatureVertecies: Position[]): boolean {
    for (const {geometry} of intersections.features) {
        const [intersetionLng, intersectionLat] = geometry.coordinates;
        const intersectionIsVertexOnLine = currentFeatureVertecies
            .some(([vertexLng, vertexLat]) => Math.abs(vertexLng - intersetionLng) < GEOJSON_VERTEX_COMPARISION_TOLERANCE 
                && Math.abs(vertexLat - intersectionLat) < GEOJSON_VERTEX_COMPARISION_TOLERANCE);
        if (!intersectionIsVertexOnLine) {
            return false;
        }
    }
    return true;
}

export function isWallInBounds(wall: Feature<WallGoometry>, bounds: BuildingGeometry): boolean {
    for(let i = 0; i < wall.geometry.coordinates.length; i++) {
        const position = wall.geometry.coordinates[i] as Position;
        if (!turf.booleanPointInPolygon(turf.point(position), bounds)) {
            return false;
        }
    }   
    const basePolyHasHoles = bounds.coordinates.length > 1;
    if (basePolyHasHoles) {
        for(let i = 1; i < bounds.coordinates.length; i++) {
            const holeCoordinates = bounds.coordinates[i];
            const intersection = turf.lineIntersect(wall, turf.polygon([holeCoordinates]));
            if (intersection.features.length >= 1) {
                return false;
            }
        }
    }
    return true;
}

export const isRoomInBounds = (room: Feature<RoomGeometry>, buildingBounds: BuildingGeometry) => turf.booleanContains(buildingBounds, room);

export function roomHasCompleteState(room: Room): boolean {
    if (hasSelfIntersection(room)) {
        return false;
    }

    const {name, id, type} = room.properties;

    if (!Object.values(RoomType).includes(type)) {
        return false;
    }

    if (name === undefined || name.trim() === "") {
        return false;
    }

    if (id === undefined || id.trim() === "") {
        return false;
    }

    return true;
}