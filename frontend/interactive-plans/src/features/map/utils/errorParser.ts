import { SerializedError, } from "@reduxjs/toolkit";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";

export default function parseFetchBuildingBoundariesError(error: FetchBaseQueryError | SerializedError): string {
    if ((error as FetchBaseQueryError).status) {
        error = error as FetchBaseQueryError;
        return 'error' in error ? error.error : JSON.stringify(error.data)
    } else {
        return (error as SerializedError).message ?? 'Произошла непредвиденная ошибка.';
    }
}
