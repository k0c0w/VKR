
import { combineReducers } from "@reduxjs/toolkit";
import { mapApi } from "@features/map";
import { plansApi } from "@features/plans";
import planEditorSlice from "@widgets/editor/lib/planEditorSlice";
import authSlice from "@features/authorization/api/authSlice";
import { authApi } from "@features/authorization";
import { aiApi } from "@features/ai";
import { catalogueApi } from "@features/catalogues";
import { itEquipmentCardApi } from "@entities/catalogue/api/itEquipmentCardApi";
import {userManagementApi} from "@features/users/api/userManagementApi";

export const rootReducer = combineReducers({
    authSlice,
    planEditorSlice,
    [authApi.reducerPath]: authApi.reducer,
    [mapApi.reducerPath]: mapApi.reducer,
    [plansApi.reducerPath]: plansApi.reducer,
    [aiApi.reducerPath]: aiApi.reducer,
    [catalogueApi.reducerPath]: catalogueApi.reducer,
    [itEquipmentCardApi.reducerPath]: itEquipmentCardApi.reducer,
    [userManagementApi.reducerPath]: userManagementApi.reducer,
});