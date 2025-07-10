import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { Position } from "geojson";

export interface IBuildingGeometryQuery {
    address: string;
}

export interface IBuildingGeometryResponse {
    type: "Polygon",
    coordinates: Position[][];
}

interface BuildingBoundariesValidationProblemDetails extends IValidationProblemDetails {
    errors: {
        address?: string[];
    };
}

export function isValidationErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: BuildingBoundariesValidationProblemDetails } {
    return (
        isFetchBaseQueryError(response) &&
        isValidationProblemDetails(response.data) &&
        "errors" in response.data &&
        "address" in response.data.errors 
    );
}

function isFetchBaseQueryError(response: FetchBaseQueryError | SerializedError): response is FetchBaseQueryError {
    return "status" in response && "data" in response;
}