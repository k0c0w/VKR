import { configureStore } from "@reduxjs/toolkit";
import { rootReducer } from "./appReducer";
import { mapApi } from "../../features/map";
import { rtkQueryErrorLogger } from "./rtkQueryErrorLogger";

const rootStore = configureStore({
    reducer: rootReducer,
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware()
        .concat(mapApi.middleware)
        .concat(rtkQueryErrorLogger)
});

export default rootStore;

export type RootState = ReturnType<typeof rootStore.getState>

export type AppDispatch = typeof rootStore.dispatch;
