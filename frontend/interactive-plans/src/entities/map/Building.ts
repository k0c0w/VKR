import { Address } from "@shared/types/ValueObjectsTypes";
import { Polygon } from "geojson";

export type Building = {
    address: Address;
    levels: number;
    boundaries: Polygon;
}