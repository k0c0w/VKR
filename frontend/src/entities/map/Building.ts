import { Address, isCompletedAddress } from "@shared/types/ValueObjectsTypes";
import { Feature, Polygon } from "geojson";
import { Level, levelHasValidState } from "./Level";

export type BuildingMetaProperties = {
    address: Address;
    name: string;
    levels: Level[];
}

export type BuildingGeometry = Polygon;

export interface Building extends Feature<BuildingGeometry, BuildingMetaProperties> {}


export function hasValidState({properties, geometry}: Building): boolean {
    const {address, levels} = properties;
    if (!isCompletedAddress(address)) {
        return false;
    }

    const metLevelNumbers = new Set<number>();
    const metLevelNames = new Set<string>();
    for(const level of levels) {
        const {name, number} = level;
        if (metLevelNames.has(name) || metLevelNumbers.has(number)) {
            return false;
        } else {
            metLevelNames.add(name);
            metLevelNumbers.add(number);
        }

        if (!levelHasValidState(level, geometry)) {
            return false;
        }
    }

    return true;
}
