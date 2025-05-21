import { CSSProperties, useEffect } from "react";
import EditBuildingBoundariesController from "../lib/EditBuildingBoundariesController";
import EditItInfrastructureController from "../lib/EditItInfrastructureController";
import EditRoomsController from "../lib/EditRoomsController";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep } from "../lib/planEditorSlice";
import FocusOnce from "../lib/FocusOnce";
import { useMap } from "react-leaflet";
import { disableAllModes, setControlsVisible } from "../lib/geoman/utils";
import GeomanPlugin from "../lib/geoman/GeomanPlugin";
import { BuildingMap } from "@shared/map";
import LevelPickController from "../lib/LevelPickController";
import MapObjectDescriptionPopup from "./MapObjectDescriptionPopup";
import { PM } from "leaflet";

interface PlanEditorWidgetProps {
    style?: CSSProperties;
    readonlyMode: boolean;
}

export default function PlanEditorWidget({style, readonlyMode}: PlanEditorWidgetProps) {
    const { building, currentLevelIndex } = useAppSelector(state => state.planEditorSlice);
    if (!building) {
        throw new Error("Initialize building first!")
    }
    const {geometry, properties} = building;
    const layer = L.geoJSON(building);
    const bounds = layer.getBounds();  
    const centroid = bounds.getCenter();      

    const levelLabels = properties.levels.map(x => x.name);

    return <BuildingMap
            style={style}
            center={centroid} 
            levelLabels={levelLabels} 
            initialLevelIndex={currentLevelIndex}
        >
            <GeomanPlugin showGeomanControls={!readonlyMode} />
            <FocusOnce bounds={bounds}/>
            <EnableButtonsAndControls readonlyMode={readonlyMode}/>
            
            <LevelPickController readonlyMode={readonlyMode} />
            <EditBuildingBoundariesController initialBoundaries={geometry}/>
            <EditRoomsController />
            <EditItInfrastructureController/>

            <MapObjectDescriptionPopup />
        </BuildingMap>
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
      editMode:        step !== CreateNewPlanStep.BuildingBoundariesSetup,
      cutPolygon:      step !== CreateNewPlanStep.InfrastructureSetup,
      rotateMode:      step !== CreateNewPlanStep.InfrastructureSetup,
    });
}

function EnableButtonsAndControls({readonlyMode}: {readonlyMode: boolean;}) {
    const map = useMap();
    const step = useAppSelector(state => state.planEditorSlice.currentStep);

    useEffect(() => {
        disableAllModes(map.pm);
        if (!readonlyMode) {
            setButtonsForStep(map.pm, step);
            
            const controlsVisible = step !== CreateNewPlanStep.BuildingBoundariesSetup;
            setControlsVisible(map.pm, controlsVisible);
        }
    }, [map, step, readonlyMode]);

    return <></>
}