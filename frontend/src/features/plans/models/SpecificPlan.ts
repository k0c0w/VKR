import { guid } from "@shared/types/guid";
import { LineString, Polygon } from "geojson";
import { ServerRoomType } from "./common";

export interface ISpecificPlan {
    id: guid;
    name: string;
    address: {
        city: string;
        street: string;
        house: string;
    };
    basementGeometry: Polygon;
    levels: Level[];
};

type Structure = {id: guid;} 
    & ({geometry: Polygon; meaning: "Room"; name: string; architectualId: string; type: ServerRoomType;}
        |{geometry:LineString; meaning:"Wall"});

type Level = {
    name?: string;
    number: number;
    structure: Structure[];
    itEquipments: {}[];
}

export interface ISpecificPlanArgs {
    id: guid;
}