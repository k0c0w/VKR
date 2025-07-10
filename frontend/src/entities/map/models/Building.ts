import { Feature, Polygon } from "geojson";
import { Level, levelHasValidState } from "./Level";
import guid from "@shared/types/guid";

export type BuildingMetaProperties = {
    region: string;
    address: string;
    name: string;
    levels: Level[];
}

export type BuildingGeometry = Polygon;

export interface Building extends Feature<BuildingGeometry, BuildingMetaProperties> {}

export type buildingId = guid;

export function hasValidState({properties, geometry}: Building): boolean {
    const {region, name, levels} = properties;
    if (!(region && name)) {
        return false;
    }

    for(const level of levels) {
        if (!levelHasValidState(level, geometry)) {
            return false;
        }
    }

    return true;
}
