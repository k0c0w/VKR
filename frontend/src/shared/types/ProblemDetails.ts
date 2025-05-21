import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { SerializedError } from "@reduxjs/toolkit";

export interface IProblemDetails {
    title: string;
    status: number;
    details?: string;
}

export interface DomainProblemDetails {
    status: number;
    title: "Domain error.";
    detail?: string;
}

export interface IValidationProblemDetails extends IProblemDetails {
    title: "One or more validation errors occurred.";
    status: 400;
    errors: {
        [err: string]: any
    } 
}

export function isDomainErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: DomainProblemDetails & { status: 400 } } {
    return (
        isFetchBaseQueryError(response) &&
        isProblemDetatils(response.data) &&
        response.data.status === 400 &&
        response.data.title === "Domain error."
    );
}

export function isNotFoundErrorResponse(
    response: FetchBaseQueryError | SerializedError
): response is FetchBaseQueryError & { data: DomainProblemDetails & { status: 404 } } {
    return (
        isFetchBaseQueryError(response) &&
        isProblemDetatils(response.data) &&
        response.data.status === 404
    );
}

export function isFetchBaseQueryError(response: FetchBaseQueryError | SerializedError): response is FetchBaseQueryError {
    return "status" in response && "data" in response;
}

export function isProblemDetatils(obj: any): obj is IProblemDetails {
    return obj.type !== undefined && typeof obj.type === 'string'
        && obj.title !== undefined && typeof obj.title === 'string'
        && obj.status !== undefined && typeof obj.status === 'number' && 100 <= obj.status && obj.status < 600;
}

export function isValidationProblemDetails(obj: any): obj is IValidationProblemDetails {
    return isProblemDetatils(obj) 
        && obj.status === 400
        && obj.title === "One or more validation errors occurred.";
}

export function isServerErrorResponse(
    response: any
): response is { status: 500; detail?: string; title: "Internal server error."; } {
    return response !== undefined && isProblemDetatils(response) && response.status === 500;
}