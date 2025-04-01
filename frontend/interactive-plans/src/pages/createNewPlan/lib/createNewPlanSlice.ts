import { Building } from "@entities/map/Building";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { Address } from "@shared/types/ValueObjectsTypes";
import { Polygon } from "geojson";

export enum CreateNewPlanStep {
    Initializing = 0,
    BuildingBoundariesSetup = 1,
    RoomsBoundariesSetup = 2,
    InfrastructureSetup = 3
}

interface CreateNewPlanState {
    building: Building;
    currentStep: CreateNewPlanStep;
};

const initialState: CreateNewPlanState = {
    building: {
        address: {
            city:"Kazan",
            houseNumber: "14",
            street: "kremlevskaya"
        },
        boundaries: {
            type: "Polygon",
            coordinates: [[[]]]
        },
        levels: 0
    },
    currentStep: CreateNewPlanStep.BuildingBoundariesSetup,
}


export const createNewPlanSlice = createSlice({
    name: "createNewPlan",
    initialState,
    reducers: {
        setStep(state, action: PayloadAction<CreateNewPlanStep>) {
            state.currentStep = action.payload
        },
        setBuilding(state, action: PayloadAction<Building>) {
            state.building = action.payload;
        }
    }
})

export default createNewPlanSlice.reducer;

export const { setStep, setBuilding } = createNewPlanSlice.actions;