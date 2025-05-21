import { Feature } from "geojson";
import { isLineString, isPoint, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { doesRoomIntersectOtherRooms, doesWallIntersetOtherWalls, FeatureWithId, hasSelfIntersection, isRoomInBounds, isWallInBounds } from "@entities/map";
import { BuildingGeometry, isITInfrastructureInBounds, ITInfrastructureGeometry, RoomGeometry, WallGoometry } from "@entities/map";
import * as turf from "@turf/turf";

function isInsideBounds(feature: Feature<WallGoometry | ITInfrastructureGeometry | RoomGeometry>, bounds: BuildingGeometry) {
    switch(true) {
        case isPoint(feature):
            return isITInfrastructureInBounds(feature as Feature<ITInfrastructureGeometry>, bounds);
        case isLineString(feature):
            return isWallInBounds(feature as Feature<WallGoometry>, bounds);
        case isPolygon(feature):
            return isRoomInBounds(feature as Feature<RoomGeometry>, bounds);
        default:
            throw new Error("Unsupported Feature geometry type.");
    }
}

export function isValidFeaturePosition({feature, bounds, walls, rooms}:{feature: FeatureWithId<WallGoometry | RoomGeometry | ITInfrastructureGeometry>; bounds: BuildingGeometry; walls:FeatureWithId<WallGoometry>[]; rooms: FeatureWithId<RoomGeometry>[]}): boolean {
    if (!isInsideBounds(feature, bounds)) {
        console.log("Вне границ помщениея", feature, bounds);
        return false;
    }
    
    if(isLineString(feature)) {
        return !(hasSelfIntersection(feature) || doesWallIntersetOtherWalls(feature, walls));
    } else if (isPolygon(feature)) {
        return !(hasSelfIntersection(feature) || doesRoomIntersectOtherRooms(feature, rooms));
    } else if (isPoint(feature)) {
        for(const wall of walls) {
            if (turf.booleanIntersects(feature, wall)) {
                return false;
            }
        }
    }

    return true;
}
