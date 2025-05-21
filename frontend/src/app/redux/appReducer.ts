
import { combineReducers } from "@reduxjs/toolkit";
import { mapApi } from "@features/map";
import { plansApi } from "@features/plans";
import planEditorSlice from "@widgets/editor/lib/planEditorSlice";

export const rootReducer = combineReducers({
    planEditorSlice,
    [mapApi.reducerPath]: mapApi.reducer,
    [plansApi.reducerPath]: plansApi.reducer,
});