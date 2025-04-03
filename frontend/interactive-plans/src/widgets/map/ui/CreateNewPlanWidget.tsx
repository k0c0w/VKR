import { Building } from "@entities/map/Building";
import { getBounds, getMapCenterByBuilding } from "../lib/utils";
import Map from "./Map";
import { CSSProperties, useEffect } from "react";
import EditBuildingBoundariesController from "../lib/EditBuildingBoundariesController";
import EditInfrastructureController from "../lib/EditInfrastructureController";
import EditRoomsBoundariesController from "../lib/EditRoomsBoundariesController";
import { useAppDispatch } from "@shared/hooks/reduxTypedHooks";
import { setBuildingBounds } from "../lib/createNewPlanSlice";
import FocusOnce from "../lib/FocusOnce";

interface CreateNewPlanWidgetProps {
    building: Building;
    style?: CSSProperties;
}

export default function CreateNewPlanWidget({building, style}: CreateNewPlanWidgetProps) {
    const {boundaries} = building;
    const dispatch = useAppDispatch();

    useEffect(() => {
        dispatch(setBuildingBounds(boundaries));
    }, [dispatch]);

    return <Map
            style={style}
            center={getMapCenterByBuilding(boundaries)}
        >
            <FocusOnce bounds={getBounds(boundaries)}/>
            <EditBuildingBoundariesController initialBoundaries={boundaries}/>
            <EditRoomsBoundariesController />
            <EditInfrastructureController/>
        </Map>
}