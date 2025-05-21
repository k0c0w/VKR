import { RoomType } from "@entities/map";
import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { guid } from "@shared/types/guid";
import { IValidationProblemDetails, isFetchBaseQueryError, isValidationProblemDetails } from "@shared/types/ProblemDetails";
import { LineString, Polygon, Position } from "geojson";

export interface ICreateNewPlanResult {
    id: guid;
};

interface IBuildingStructure {
    geometry: LineString | Polygon;
}

type Structure = IBuildingStructure & ({meaning: "Room"; name: string; architectualId: string; type: RoomType;}|{meaning:"Wall"});

type Level = {
    name?: string;
    number: number;
    structure: Structure[];
    itEquipments: {}[];
}

export interface ICreateNewPlanArgs {
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
