
import { combineReducers } from "@reduxjs/toolkit";
import { mapApi } from "@features/map";
import { createNewPlanReducer } from "@widgets/map";

export const rootReducer = combineReducers({
    createNewPlanReducer,
    [mapApi.reducerPath]: mapApi.reducer
});