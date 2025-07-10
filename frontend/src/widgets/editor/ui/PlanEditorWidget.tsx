import { useEffect } from "react";
import EditBuildingBoundariesController from "../lib/EditBuildingBoundariesController";
import EditItEquipmentController from "../lib/EditItEquipmentController";
import EditRoomsController from "../lib/EditRoomsController";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, setBuildingStructureOnLevel } from "../lib/planEditorSlice";
import { PM } from "leaflet";
import AutomaticImageLabelingPlugin from "../lib/AutomaticImageLabelingPlugin";
import { disableAllModes, setControlsVisible } from "../lib/helpers";
import { useBuildingMap } from "@shared/map";
import RoomDescriptionPopup from "@widgets/editor/ui/RoomPopup";
import PlanViewerBaseWidget from "./PlanViewerBaseWidget";
import ItEquipmentPopup from "@widgets/editor/ui/ItEquipmentPopup";

export default function PlanEditorWidget() {
    const {building, buildingInfoFromCatalogue} = useAppSelector(state => state.planEditorSlice);
    const dispatch = useAppDispatch();
    if (!building) {
        throw new Error("Initialize building first!")
    }

    return <PlanViewerBaseWidget
            building={building}
            readonlyMode={false}
        >
            <EnableButtonsAndControls />
            
            <EditBuildingBoundariesController />
            <EditRoomsController />
            <EditItEquipmentController/>
            <AutomaticImageLabelingPlugin />

            <RoomDescriptionPopup
                catalogue={buildingInfoFromCatalogue}
                onUpdate={(e) => dispatch(setBuildingStructureOnLevel({levelIndex: e.level, featureId: e.prevId, feature: e.update}))}
            />
            <ItEquipmentPopup/>
        </PlanViewerBaseWidget>
}

function setButtonsForStep(pm: PM.PMMap, step: CreateNewPlanStep) {
    pm.removeControls();
    pm.addControls({
      position: 'topleft',
      drawMarker:      step === CreateNewPlanStep.InfrastructureSetup,
      drawPolyline:    step === CreateNewPlanStep.RoomsBoundariesSetup,
      drawPolygon:     step === CreateNewPlanStep.RoomsBoundariesSetup,
      dragMode:        step !== CreateNewPlanStep.BuildingBoundariesSetup,
      removalMode:     step !== CreateNewPlanStep.BuildingBoundariesSetup,
      editMode:        step === CreateNewPlanStep.RoomsBoundariesSetup,
      rotateMode:      step !== CreateNewPlanStep.InfrastructureSetup,
    });
}

function EnableButtonsAndControls() {
    const {map} = useBuildingMap();
    const step = useAppSelector(state => state.planEditorSlice.currentStep);

    useEffect(() => {
        disableAllModes(map.pm);
        setButtonsForStep(map.pm, step);
        
        const controlsVisible = step !== CreateNewPlanStep.BuildingBoundariesSetup;
        setControlsVisible(map.pm, controlsVisible);
    }, [map, step]);

    return <></>
}