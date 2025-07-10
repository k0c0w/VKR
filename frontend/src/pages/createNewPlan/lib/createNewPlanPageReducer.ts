import { ReactNode, useReducer } from "react";
import { CreateNewPlanPageSteps } from "./createNewPlanPageSteps";
import { useAppDispatch } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, initNewState } from "@widgets/editor";
import { Polygon } from "geojson";
import { CatalogueBuilding } from "@entities/catalogue";

type CreatePlanErrorSummary = {
  title: string;
  payload?: ReactNode;
};

type BuildingFormData = {
  region: string;
  name: string;
  address: string;
};

type CreatePlanAction =
  | { type: "ON_SelectBuildingForPlan_COMPLETE"; payload: BuildingFormData }
  | {type: "ON_LoadBuildingInformation_COMPLETE"; }
  | { type: "SET_ERROR"; payload: CreatePlanErrorSummary | undefined }
  | { type: "RESET" };

interface CreatePlanStateBase {
  step: CreateNewPlanPageSteps;
  buildingData: BuildingFormData | undefined;
  error: CreatePlanErrorSummary | undefined;
}

type InitialState = CreatePlanStateBase & { step: CreateNewPlanPageSteps.SelectBuildingForPlan };

type CreatePlanStateWhenSelectBuildingForPlanComplete = CreatePlanStateBase & {
    step:  CreateNewPlanPageSteps.LoadBuildingInformation,
    buildingData: BuildingFormData;
}

type CreatePlanStateWhenLoadBuildingInformationComplete = CreatePlanStateBase & {
    step:  CreateNewPlanPageSteps.OpenPlanEditor,
}

type CreatePlanState =
    | InitialState 
    | CreatePlanStateWhenSelectBuildingForPlanComplete 
    | CreatePlanStateWhenLoadBuildingInformationComplete;

const initialState: InitialState = {
  step: CreateNewPlanPageSteps.SelectBuildingForPlan,
  buildingData: undefined,
  error: undefined,
};

function createPlanReducer(state: CreatePlanState, action: CreatePlanAction): CreatePlanState {
  switch (action.type) {
    case "ON_SelectBuildingForPlan_COMPLETE":
      return { ...state, buildingData: action.payload, step: CreateNewPlanPageSteps.LoadBuildingInformation, };
    case "ON_LoadBuildingInformation_COMPLETE":
      return { ...state, step: CreateNewPlanPageSteps.OpenPlanEditor,};
    case "SET_ERROR":
      return { ...state, error: action.payload };
    case "RESET":
      return { ...initialState };
    default:
      return state;
  }
}

export default function useCreateNewPlanReducer() {
  const appDispatch = useAppDispatch();
  const [state, dispatch] = useReducer(createPlanReducer, initialState);

  return {
    state,

    handleBuildingSelect: (data: BuildingFormData) => dispatch({ type: "ON_SelectBuildingForPlan_COMPLETE", payload: data }),
    handleBuildingLoad: ({basementGeometry, buildingInfo, region}: {basementGeometry: Polygon; buildingInfo: CatalogueBuilding; region: string;}) => {
        appDispatch(initNewState({
                basementGeometry,
                buildingInfoFromCatalogue: buildingInfo,
                step: CreateNewPlanStep.BuildingBoundariesSetup,
                region: region
            }))
        dispatch({type: "ON_LoadBuildingInformation_COMPLETE"})
    },
    setError: (d: CreatePlanErrorSummary | undefined) => dispatch({type: "SET_ERROR", payload: d}),
  };
}