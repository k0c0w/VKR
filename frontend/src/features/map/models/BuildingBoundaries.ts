import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { Position } from "geojson";

export interface IBuildingBoundariesQuery {
    city: string;
    street: string;
    house: string;
}

export interface IBuildingBoundariesResponse {
    levelsCount: number;
    address: string;
    geometry: {
        type: "Polygon",
        coordinates: Position[][];
    }
}

interface BuildingBoundariesValidationProblemDetails extends IValidationProblemDetails {
    errors: {
        City?: string[];
        Street?: string[];
        House?: string[];
    };
}

export function isSuccessResponse(response: any): response is IBuildingBoundariesResponse {
    return "levelsCount" in response &&
        typeof response.levelsCount === "number" &&
        "address" in response &&
        "geometry" in response &&
        "type" in response.geometry && response.geometry.type === "Polygon" &&
        "coordinates" in response.geometry
}

export function isValidationErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: BuildingBoundariesValidationProblemDetails } {
    return (
        isFetchBaseQueryError(response) &&
        isValidationProblemDetails(response.data) &&
        "errors" in response.data &&
        typeof response.data.errors === "object" &&
        response.data.errors !== null &&
        (Array.isArray(response.data.errors.City) ||
         Array.isArray(response.data.errors.Street) ||
         Array.isArray(response.data.errors.House))
    );
}

function isFetchBaseQueryError(response: FetchBaseQueryError | SerializedError): response is FetchBaseQueryError {
    return "status" in response && "data" in response;
}