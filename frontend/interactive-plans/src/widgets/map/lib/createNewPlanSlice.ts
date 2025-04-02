import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { LatLngLiteral } from "leaflet";

export enum CreateNewPlanStep {
    BuildingBoundariesSetup = 0,
    RoomsBoundariesSetup = 1,
    InfrastructureSetup = 2,
}

interface CreateNewPlanState {
    buildingBounds: LatLngLiteral[][] | undefined;
    currentStep: CreateNewPlanStep;
};

const initialState: CreateNewPlanState = {
    buildingBounds: undefined,
    currentStep: CreateNewPlanStep.BuildingBoundariesSetup,
}

export const createNewPlanSlice = createSlice({
    name: "createNewPlan",
    initialState,
    reducers: {
        setStep(state, action: PayloadAction<CreateNewPlanStep>) {
            state.currentStep = action.payload
        },
        setBuildingBounds(state, action: PayloadAction<LatLngLiteral[][]>) {
            state.buildingBounds = action.payload;
        }
    }
})

export default createNewPlanSlice.reducer;

export const { setStep, setBuildingBounds } = createNewPlanSlice.actions;