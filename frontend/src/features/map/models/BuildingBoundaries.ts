import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { isProblemDetatils, isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { Position } from "geojson";

export interface IBuildingBoundariesQuery {
    city: string;
    street: string;
    house: string;
}

export interface IBuildingBoundariesResponse {
    levelsCount: number;
    address: string;
    geometry: Position[][];
}

interface BuildingBoundariesValidationProblemDetails extends IValidationProblemDetails {
    errors: {
        City?: string[];
        Street?: string[];
        House?: string[];
    };
}

interface DomainProblemDetails {
    status: number;
    title: "Доменная ошибка.";
    detail?: string;
}

export function isSuccessResponse(response: unknown): response is IBuildingBoundariesResponse {
    return (
        typeof response === "object" &&
        response !== null &&
        "levelsCount" in response &&
        typeof (response as any).levelsCount === "number" &&
        "address" in response &&
        typeof (response as any).address === "string" &&
        "geometry" in response &&
        Array.isArray((response as any).geometry) &&
        (response as any).geometry.every((poly: any) => Array.isArray(poly) && poly.every((pos: any) => Array.isArray(pos)))
    );
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

export function isDomainErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: DomainProblemDetails & { status: 400 } } {
    return (
        isFetchBaseQueryError(response) &&
        isProblemDetatils(response.data) &&
        response.data.status === 400 &&
        response.data.title === "Доменная ошибка."
    );
}

export function isNotFoundErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: DomainProblemDetails & { status: 404 } } {
    return (
        isFetchBaseQueryError(response) &&
        isProblemDetatils(response.data) &&
        response.data.status === 404 &&
        response.data.title === "Доменная ошибка."
    );
}

function isFetchBaseQueryError(response: FetchBaseQueryError | SerializedError): response is FetchBaseQueryError {
    return "status" in response && "data" in response;
}