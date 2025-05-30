import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { guid } from "@shared/types/guid";
import {  isFetchBaseQueryError, isValidationProblemDetails } from "@shared/types/ProblemDetails";
import { LineString, Polygon } from "geojson";
import { ServerRoomType } from "./common";

export interface ICreateNewPlanResult {
    id: guid;
};

interface IBuildingStructure {
    geometry: LineString | Polygon;
}

type Structure = IBuildingStructure & ({meaning: "Room"; name: string; architectualId: string; type: ServerRoomType;}|{meaning:"Wall"});

type Level = {
    name?: string;
    number: number;
    structure: Structure[];
    itEquipments: {}[];
}

export interface ICreateNewPlanArgs {
    name: string;
    address: {
        city: string;
        street: string;
        house: string;
    };
    basementGeometry: Polygon;
    levels: Level[];
}

interface PlanValidationErrors {
    "Address.City"?: string[];
    "Address.Street"?: string[];
    "Address.House"?:string[];
}


export function isValidationErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data:  {errors: PlanValidationErrors}} {
    return (
        isFetchBaseQueryError(response) &&
        isValidationProblemDetails(response.data) &&
        "errors" in response.data
    );
}
