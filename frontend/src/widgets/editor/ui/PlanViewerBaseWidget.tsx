import FocusOnce from "../lib/FocusOnce";
import { BuildingMap } from "@shared/map";
import { Building } from "@entities/map";
import LevelPickController from "../lib/LevelPickController";
import { ReactNode } from "react";
import { PlanViewerContextProvider } from "./PlanViewerContext";

export default function PlanViewerBaseWidget({building, children, readonlyMode=true}: {building: Building; readonlyMode?: boolean; children?: ReactNode;}) {
    const layer = L.geoJSON(building);
    const bounds = layer.getBounds();  
    const centroid = bounds.getCenter();      

    return (
        <BuildingMap
            center={centroid}
            initialLevelIndex={0}
            maxBounds={readonlyMode ? bounds : undefined} 
            building={building}
        >
            <PlanViewerContextProvider>
                <FocusOnce bounds={bounds}/>
                <LevelPickController building={building} readonlyMode={readonlyMode}/>
                {children}
            </PlanViewerContextProvider>
        </BuildingMap>);
}
