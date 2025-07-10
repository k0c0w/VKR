import { isPoint } from "@shared/types/geoJsonTypeGuards";
import { Feature, GeoJsonProperties, Point } from "geojson";
import { FeatureWithId } from "./common"
import { BuildingGeometry } from "./Building";
import * as turf from "@turf/turf";
import { Room, RoomGeometry } from "./BuildingStructure";
import { GEOJSON_PRECISION } from "@app/config/constants";

export type ItEquipmentId = string;

export type ITEquipmentMetaProperties = {
    name: string;
    linkedToAudienceId: number;
    cardUrl: string;
    historyUrl: string;
    meaning: "IT-Infrastructure";
};

export type ITEquipmentGeometry = Point;
export interface ITEquipment extends FeatureWithId<ItEquipmentId, ITEquipmentGeometry, ITEquipmentMetaProperties> {}

export function isITEquipment(feature: Feature): feature is Feature<ITEquipmentGeometry, ITEquipmentMetaProperties> {
    return hasITEquipmentGeometry(feature) && isITEquipmentMetaProperties(feature.properties);
}

export function hasITEquipmentGeometry(feature: Feature): feature is Feature<ITEquipmentGeometry> {
    return isPoint(feature);
}

export function isITEquipmentMetaProperties(props: GeoJsonProperties): props is ITEquipmentMetaProperties {
    return props !== null && props.meaning === "IT-Infrastructure";
}

export const isITEquipmentInBounds = (infrasrtucture: Feature<ITEquipmentGeometry>, buildingBounds: BuildingGeometry) => turf.booleanPointInPolygon(infrasrtucture, buildingBounds);

export const isITEquipmentInsideRoom = (equipment: Feature<ITEquipmentGeometry>, room: Feature<RoomGeometry>) => {
  const truncatedEquipment = turf.truncate(equipment, { precision: GEOJSON_PRECISION });
  const truncatedRoom = turf.truncate(room, { precision: GEOJSON_PRECISION });
  return turf.booleanPointInPolygon(truncatedEquipment.geometry, truncatedRoom);
};

export const hasCorrectPosition = function (infrastructure: ITEquipment, levelRooms: Room[]) {
  const belongingRoom = levelRooms.find(x => x.id === infrastructure.properties.linkedToAudienceId);
  if (!belongingRoom) {
    return false;
  }
  return isITEquipmentInsideRoom(infrastructure, belongingRoom);
};

function hasValidProperties({name, linkedToAudienceId}: ITEquipmentMetaProperties): boolean {
    return name.trim() !== "" && linkedToAudienceId != 0;
}

export function hasCompleteState({properties}: ITEquipment): boolean {
    return hasValidProperties(properties);
}
