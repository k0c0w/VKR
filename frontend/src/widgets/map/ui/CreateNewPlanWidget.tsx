import { getBounds } from "@shared/map/lib/leafletUtilsAdditions";
import { CSSProperties, useEffect } from "react";
import EditBuildingBoundariesController from "../lib/EditBuildingBoundariesController";
import EditItInfrastructureController from "../lib/EditItInfrastructureController";
import EditRoomsController from "../lib/EditRoomsController";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep } from "../lib/createNewPlanSlice";
import FocusOnce from "./FocusOnce";
import { useMap } from "react-leaflet";
import { disableAllModes, setButtonsForStep, setControlsVisible } from "../lib/geoman/utils";
import GeomanPlugin from "../lib/geoman/GeomanPlugin";
import { BuildingMap } from "@shared/map";
import LevelPickController from "../lib/LevelPickController";
import MapObjectDescriptionPopup from "../ui/MapObjectDescriptionPopup";

interface CreateNewPlanWidgetProps {
    style?: CSSProperties;
}

export default function CreateNewPlanWidget({style}: CreateNewPlanWidgetProps) {
    const { building, currentLevelIndex } = useAppSelector(state => state.createNewPlanReducer);
    if (!building) {
        throw new Error("Initialize building first!")
    }
    const {geometry, properties} = building;
    const layer = L.geoJSON(building);
    const bounds = layer.getBounds();            // LatLngBounds
    const centroid = bounds.getCenter();         // LatLng

    const levelLabels = properties.levels.map(x => x.name);

    return <BuildingMap
            style={style}
            center={centroid} 
            levelLabels={levelLabels} 
            initialLevelIndex={currentLevelIndex}
        >
            <GeomanPlugin showGeomanControls={true} />
            <FocusOnce bounds={bounds}/>
            <EnableButtonsAndControls />
            
            <LevelPickController />
            <EditBuildingBoundariesController initialBoundaries={geometry}/>
            <EditRoomsController />
            <EditItInfrastructureController/>

            <MapObjectDescriptionPopup />
        </BuildingMap>
}

function EnableButtonsAndControls() {
    const map = useMap();
    const step = useAppSelector(state => state.createNewPlanReducer.currentStep);

    useEffect(() => {
        disableAllModes(map.pm);
        setButtonsForStep(map.pm, step);
 
        const controlsVisible = step !== CreateNewPlanStep.BuildingBoundariesSetup;
        setControlsVisible(map.pm, controlsVisible);
    }, [map, step]);

    useEffect(() => {
        map.on("levelpicker:changelevel", (e) => console.log(e));

    }, [map]);

    return <></>
}