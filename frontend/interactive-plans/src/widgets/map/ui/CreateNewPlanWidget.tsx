import { Building } from "@entities/map/Building";
import { getMapCenterByBuilding, mapGeoJsonPolygonToLeafletExpression } from "../lib/utils";
import Map from "./Map";
import { CSSProperties } from "react";
import EditBuildingBoundariesController from "../lib/EditBuildingBoundariesController";
import EditInfrastructureController from "../lib/EditInfrastructureController";
import EditRoomsBoundariesController from "../lib/EditRoomsBoundariesController";

interface CreateNewPlanWidgetProps {
    building: Building;
    style?: CSSProperties;
}

export default function CreateNewPlanWidget({building, style}: CreateNewPlanWidgetProps) {
    const {boundaries} = building;
    
    return <Map
            style={style}
            center={getMapCenterByBuilding(building.boundaries)}
        >
            <EditBuildingBoundariesController initialBoundaries={mapGeoJsonPolygonToLeafletExpression(boundaries)}/>
            <EditRoomsBoundariesController/>
            <EditInfrastructureController/>
        </Map>
}