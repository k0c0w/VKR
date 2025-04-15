import { Address } from "@shared/types/ValueObjectsTypes";
import { Feature, Polygon } from "geojson";
import { Level } from "./Level";

export type BuildingMetaProperties = {
    address: Address;
    levels: Level[];
}

export interface Building extends Feature<Polygon, BuildingMetaProperties> {}