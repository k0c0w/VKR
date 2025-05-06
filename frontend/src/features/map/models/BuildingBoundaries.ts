import { IProblemDetails, isProblemDetatils, isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { Position } from "geojson";

export interface IBuildingBoundariesQuery {
    city: string;
    street: string;
    house: string;
}

export type BuildingBoundariesResponse = BuildingBoundariesSuccess | BuildingBoundariesError;

type BuildingBoundariesSuccess = {
    levelsCount: number;
    address: string;
    geometry: Position[][];
}

type BuildingBoundariesValidationProblemDetails = IValidationProblemDetails & {errors: {City?:string[]; Street?:string[]; House?:string[];}}

type BuildingBoundariesError = IProblemDetails | BuildingBoundariesValidationProblemDetails;


export function isSuccessResponse(response: BuildingBoundariesResponse): response is BuildingBoundariesSuccess {
    return response !== undefined && !isProblemDetatils(response) 
        && response.levelsCount !== undefined && response.geometry != undefined && response.address !== undefined;
}

export function isValidationErrorResponse(
    response: BuildingBoundariesResponse
): response is BuildingBoundariesValidationProblemDetails {

    return isValidationProblemDetails(response) 
        && (response.errors.City?.length != undefined || response.errors.Street?.length != undefined || response.errors.House?.length != undefined);
}

export function isDomainErrorResponse(
    response: BuildingBoundariesResponse
): response is {
    status: 400;
    title: "Доменная ошибка.";
    detail?: string;
} {
    return response !== undefined && isProblemDetatils(response) && response.status === 400 && response.title === "Доменная ошибка.";
}

export function isNotFoundErrorResponse(
    response: BuildingBoundariesResponse
): response is {
    status: 404;
    title: "Domain error.";
    detail?: string;
} {
    return response !== undefined && isProblemDetatils(response) && response.title === "Доменная ошибка." && response.status === 404;
}
