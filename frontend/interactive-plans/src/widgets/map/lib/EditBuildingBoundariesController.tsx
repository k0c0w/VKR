import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks"
import { CreateNewPlanStep, setBuildingBounds } from "./createNewPlanSlice";
import BuildingBasePolygon from "./BuildingBasePolygon";
import { useEffect, useRef } from "react";
import { LatLngLiteral, Polygon as LeafletPolygon } from "leaflet";
import { Polygon as GeoJsonPolygon } from "geojson";
import { useMap } from "react-leaflet";
import {disableAllModes, setControlsVisible} from "./geoman/utils";
import { mapGeoJsonPolygonToLeafletExpression, mapLeafletExpressionToGeoJsonPolygon } from "./utils";

/* TODO: добавить возможность вырезать полости из полигона */
export default function EditBuildingBoundariesController({initialBoundaries}: {initialBoundaries: GeoJsonPolygon}) {
    const dispatch = useAppDispatch();
    const ref = useRef<LeafletPolygon| null>(null);
    const { currentStep, buildingBounds } = useAppSelector(state => state.createNewPlanReducer);
    const map = useMap();

    function handleShapeChange(geometry: LatLngLiteral[][]) {
        const polygon = mapLeafletExpressionToGeoJsonPolygon(geometry);
        dispatch(setBuildingBounds(polygon));
    }

    useEffect(() => {
        if (currentStep === CreateNewPlanStep.BuildingBoundariesSetup) {
            const pm = map.pm;

            setControlsVisible(pm, false);
            disableAllModes(pm);
        }
    }, [currentStep, map]);

    return <BuildingBasePolygon
        ref={ref}
        editable={currentStep === CreateNewPlanStep.BuildingBoundariesSetup}
        positions={buildingBounds ? mapGeoJsonPolygonToLeafletExpression(buildingBounds) : mapGeoJsonPolygonToLeafletExpression(initialBoundaries)}
        onChange={handleShapeChange}
    />
}