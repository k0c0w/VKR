import { configureStore } from "@reduxjs/toolkit";
import { rootReducer } from "./appReducer";
import { mapApi } from "@features/map";
import { rtkQueryErrorLogger } from "./rtkQueryErrorLogger";
import { plansApi } from "@features/plans";
import { authApi } from "@features/authorization";
import { aiApi } from "@features/ai";

const rootStore = configureStore({
    reducer: rootReducer,
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware()
        .concat(authApi.middleware)
        .concat(mapApi.middleware)
        .concat(plansApi.middleware)
        .concat(aiApi.middleware)
        .concat(rtkQueryErrorLogger)
});

export default rootStore;

export type RootState = ReturnType<typeof rootStore.getState>

export type AppDispatch = typeof rootStore.dispatch;
