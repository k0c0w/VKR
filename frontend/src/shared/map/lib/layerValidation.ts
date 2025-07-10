import { Feature } from "geojson";
import { isLineString, isPoint, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { doesRoomIntersectOtherRooms, doesWallIntersectOtherWalls, FeatureWithId, hasSelfIntersection, isRoom, isRoomInBounds, isWall, isWallInBounds, Room, RoomId, RoomType, Wall, WallId } from "@entities/map";
import { BuildingGeometry, RoomGeometry, WallGeometry } from "@entities/map";
import * as turf from "@turf/turf";
import { isITEquipmentInBounds, ItEquipmentId, ITEquipmentGeometry } from "@entities/map/models/ItEquipment";
import { isKnownShapeLayer, toGeoJsonWithId } from "./leafletUtilsAdditions";
import { LayerWithFeatureId } from "./leafletTypeExtensions";

function isInsideBounds(feature: Feature<WallGeometry | ITEquipmentGeometry | RoomGeometry>, bounds: BuildingGeometry) {
    switch(true) {
        case isPoint(feature):
            return isITEquipmentInBounds(feature as Feature<ITEquipmentGeometry>, bounds);
        case isLineString(feature):
            return isWallInBounds(feature as Feature<WallGeometry>, bounds);
        case isPolygon(feature):
            return isRoomInBounds(feature as Feature<RoomGeometry>, bounds);
        default:
            throw new Error("Unsupported Feature geometry type.");
    }
}

export function isValidFeaturePosition({feature, bounds, walls, rooms}: {
        feature: FeatureWithId<(ItEquipmentId | WallId | RoomId), WallGeometry | RoomGeometry | ITEquipmentGeometry>;
        bounds: BuildingGeometry; 
        walls:FeatureWithId<WallId, WallGeometry>[]; 
        rooms: FeatureWithId<RoomId, RoomGeometry>[]}): boolean {
    if (!isInsideBounds(feature, bounds)) {
        return false;
    }
    
    if(isLineString(feature)) {
        return !(hasSelfIntersection(feature) || doesWallIntersectOtherWalls(feature, walls));
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
