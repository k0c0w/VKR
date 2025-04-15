import { isPoint } from "@shared/types/geoJsonTypeGuards";
import { Feature, GeoJsonProperties, Point } from "geojson";
import { FeatureWithId } from "./common";

export type ITInfrastructureMetaProperties = {
    name: string;
    inventoryNumber: string;
    serialNumber: string;
    meaning: "IT-Infrastructure";
};

export type ITInfrastructureGeometry = Point;
export interface ITInfrastructure extends FeatureWithId<ITInfrastructureGeometry, ITInfrastructureMetaProperties> {}

export function isITInfrastructure(feature: Feature): feature is Feature<ITInfrastructureGeometry, ITInfrastructureMetaProperties> {
    return isPoint(feature) && isITInfrastructureMetaProperties(feature.properties);
}

export function isITInfrastructureMetaProperties(props: GeoJsonProperties): props is ITInfrastructureMetaProperties {
    return props !== null && props.meaning === "IT-Infrastructure";
}