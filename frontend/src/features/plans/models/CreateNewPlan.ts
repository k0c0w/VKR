import guid from "@shared/types/guid";
import { LineString, Polygon, Point } from "geojson";
import { ServerRoomType } from "./common";

export interface ICreateNewPlanResult {
    id: guid;
};

interface IBuildingStructure {
    geometry: LineString | Polygon;
}

type Structure = IBuildingStructure & ({meaning: "Room"; name: string; id: number; type: ServerRoomType;}|{meaning:"Wall"});

type ItEquipment = {
    relatedToRoomId: number;
    inventoryNumber: string;
    locationPoint: Point;
}

type Level = {
    name?: string;
    number: number;
    structure: Structure[];
    itEquipments: ItEquipment[];
}

export interface ICreateNewPlanArgs {
    buildingName: string;
    address: string;
    region: string;
    basementGeometry: Polygon;
    levels: Level[];
}

