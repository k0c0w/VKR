import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, setStep, setBaseBounds } from "./createNewPlanSlice";
import { Polygon } from "geojson";

export default function useSlice() {
    const state = useAppSelector(state => state.createNewPlanReducer)
    const dispatch = useAppDispatch();

    return {
        state,
        setStep: (step: CreateNewPlanStep) => dispatch(setStep(step)),
        setBaseBounds: (bounds: Polygon) => dispatch(setBaseBounds(bounds)),
    }
}