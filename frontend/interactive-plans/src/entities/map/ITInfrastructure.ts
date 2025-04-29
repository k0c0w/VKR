import { isPoint } from "@shared/types/geoJsonTypeGuards";
import { Feature, GeoJsonProperties, Point } from "geojson";
import { FeatureWithId } from "./common";
import { BuildingGeometry } from "./Building";
import * as turf from "@turf/turf";

export type ITInfrastructureMetaProperties = {
    name: string;
    inventoryNumber: string;
    serialNumber: string;
    linkedToAudienceId: string;
    meaning: "IT-Infrastructure";
};

export type ITInfrastructureGeometry = Point;
export interface ITInfrastructure extends FeatureWithId<ITInfrastructureGeometry, ITInfrastructureMetaProperties> {}

export function isITInfrastructure(feature: Feature): feature is Feature<ITInfrastructureGeometry, ITInfrastructureMetaProperties> {
    return hasITInfrastructureGeometry(feature) && isITInfrastructureMetaProperties(feature.properties);
}

export function hasITInfrastructureGeometry(feature: Feature): feature is Feature<ITInfrastructureGeometry> {
    return isPoint(feature);
}

export function isITInfrastructureMetaProperties(props: GeoJsonProperties): props is ITInfrastructureMetaProperties {
    return props !== null && props.meaning === "IT-Infrastructure";
}

export const isITInfrastructureInBounds = (infrasrtucture: Feature<ITInfrastructureGeometry>, buildingBounds: BuildingGeometry) => turf.booleanPointInPolygon(infrasrtucture, buildingBounds);

function hasValidProperties({inventoryNumber, serialNumber, name, linkedToAudienceId}: ITInfrastructureMetaProperties): boolean {
    return inventoryNumber.trim() !== "" && serialNumber.trim() !== "" && name.trim() !== "" && linkedToAudienceId !== "";
}

export function hasCompleteState({properties}: ITInfrastructure): boolean {
    return hasValidProperties(properties);
}
