import { mapGeoJsonPolygonToLeafletExpression } from "@shared/map/lib/leafletUtilsAdditions";
import { useBuildingMap } from "./BuildingMapContext";
import { Polygon as ReactLeafletPolygon } from "react-leaflet";
import { BuildingMapPanes } from "@shared/map";
import { basementStyle } from "@shared/map/lib/styling/styling";

export default function BuildingBasementPolygonLayer() {
    const { building, basementPolygonRef } = useBuildingMap();
    
    return <ReactLeafletPolygon 
                ref={basementPolygonRef} 
                positions={mapGeoJsonPolygonToLeafletExpression(building.geometry)} 
                pathOptions={basementStyle} 
                pane={BuildingMapPanes.buildingBasement.pane} 
           />;
}
