import { SerializedError } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/query/react";
import { isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";

export interface ISignInArgs {
    login: string;
    password: string;
}

export interface ISignInResult {
    roles: ['user'] & string[];
    p1: string;
    p2: string;
    p_h: string;
}

export function isSignInArgsValidationProblem(object: FetchBaseQueryError | SerializedError): object is FetchBaseQueryError & {data: IValidationProblemDetails & {errors: {email?: string[]; password?: string[]}}} {
    return isValidationProblemDetails(object);
}