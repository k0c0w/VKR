import PlanEditorWidget from "./ui/PlanEditorWidget";
import PlanEditorStepperWidget from "./ui/PlanEditorStepperWidget";
import { addLevelAndSwitchOnIt, editBuilding, editCurrentLevel, focusOnFeature, initNewState, removeCurrentLevel, resetToInitialState, setBuildingStructureOnCurrentLevel, setCurrentLevelIndex, setItInfrastructureOnCurrentLevel, setStep, updateMetaProperties } from "./lib/planEditorSlice";

export { PlanEditorWidget, PlanEditorStepperWidget };


/* Map edit mode switching */ 
export { setStep, resetToInitialState, initNewState, focusOnFeature, editBuilding, setBuildingStructureOnCurrentLevel, setItInfrastructureOnCurrentLevel, updateMetaProperties, addLevelAndSwitchOnIt, removeCurrentLevel, setCurrentLevelIndex, editCurrentLevel}
