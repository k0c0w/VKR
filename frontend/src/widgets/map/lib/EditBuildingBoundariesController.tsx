import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks"
import { CreateNewPlanStep, editBuilding } from "./createNewPlanSlice";
import BuildingBasePolygon from "../../../shared/map/ui/BuildingBasePolygon";
import { useEffect, useRef } from "react";
import { Polygon as LeafletPolygon } from "leaflet";
import { Polygon as GeoJsonPolygon } from "geojson";
import { useMap } from "react-leaflet";
import {disableAllModes, setControlsVisible} from "./geoman/utils";
import { mapGeoJsonPolygonToLeafletExpression } from "@shared/map/lib/leafletUtilsAdditions";
import { Building } from "@entities/map";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION } from "@app/config/constants";

export default function EditBuildingBoundariesController({initialBoundaries}: {initialBoundaries: GeoJsonPolygon}) {
    const dispatch = useAppDispatch();
    const ref = useRef<LeafletPolygon| null>(null);
    const { currentStep, building } = useAppSelector(state => state.createNewPlanReducer);
    const map = useMap();

    function handleShapeChange(buildingBounds: Building) {
        const roundedGeometry = roundCoordinates(buildingBounds.geometry, GEOJSON_PRECISION);
        dispatch(editBuilding({ geometry: roundedGeometry }));
    }

    useEffect(() => {
        if (map && currentStep === CreateNewPlanStep.BuildingBoundariesSetup) {
            const pm = map.pm;

            setControlsVisible(pm, false);
            disableAllModes(pm);
        } else {

        }
    }, [currentStep, map]);

    return <BuildingBasePolygon
        ref={ref}
        editable={currentStep === CreateNewPlanStep.BuildingBoundariesSetup}
        positions={building ? mapGeoJsonPolygonToLeafletExpression(building.geometry) : mapGeoJsonPolygonToLeafletExpression(initialBoundaries)}
        onChange={handleShapeChange}
    />
}