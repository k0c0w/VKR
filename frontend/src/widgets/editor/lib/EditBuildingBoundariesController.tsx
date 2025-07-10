import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks"
import { CreateNewPlanStep, editBuilding } from "./planEditorSlice";
import { useEffect } from "react";
import { Polygon as LeafletPolygon, PM } from "leaflet";
import { Feature, Polygon as GeoJsonPolygon } from "geojson";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { useBuildingMap } from "@shared/map/ui/BuildingMapContext";
import { disableAllModes, setControlsVisible } from "./helpers";

export default function EditBuildingBoundariesController() {
    const dispatch = useAppDispatch();
    const { currentStep } = useAppSelector(state => state.planEditorSlice);
    const controllerFunctionsActive = currentStep === CreateNewPlanStep.BuildingBoundariesSetup;
    const { map, basementPolygonRef } = useBuildingMap();

    function handleShapeChange(buildingBounds: Feature<GeoJsonPolygon>) {
        const roundedGeometry = roundCoordinates(buildingBounds.geometry, GEOJSON_PRECISION);
        dispatch(editBuilding({ geometry: roundedGeometry }));
    }

    useEffect(() => {
        if (map && controllerFunctionsActive) {
            const pm = map.pm;

            setControlsVisible(pm, false);
            disableAllModes(pm);
        } else {

        }
    }, [controllerFunctionsActive, map]);

    useEffect(() => {
        if (!map.levelControl) {
            return;
        } else {
            map.levelControl.setControlDisabled(controllerFunctionsActive);
        }
    }, [map, controllerFunctionsActive]);

    useEffect(() => {
        const basement = basementPolygonRef.current;
        const pmEditHandler: PM.EditEventHandler =  function (e) {
            if (e.shape !== "Polygon") {
                throw new Error("Only 'Polygon' is supported for BuildingBase!");
            }

            const polygonLayer = e.layer as LeafletPolygon;
            const polygon = polygonLayer.toGeoJSON(GEOJSON_PRECISION)
            if (polygon.geometry.type !== "Polygon") {
                throw new Error("Unsupported polygon shape!");
            }

            handleShapeChange(polygon as Feature<GeoJsonPolygon>);
        };

        basement?.on("pm:edit", pmEditHandler);

        return () => {
            basement?.off("pm:edit", pmEditHandler);
        }

    }, [basementPolygonRef]);

    useEffect(() => {
        const polygon = basementPolygonRef.current;
        if (polygon) {
            if (controllerFunctionsActive) {
                polygon.pm.enable({
                    allowSelfIntersection: false,
                    allowSelfIntersectionEdit: false,
                    removeLayerBelowMinVertexCount: false,
                    allowRemoval: false,
                    allowCutting: false,
                    allowRotation: true,
                    draggable: false,
                    snappable: false,
                    allowEditing: true,
                });
            } else {
                polygon.pm.disable();
                polygon.pm.setOptions({
                    draggable: false,
                    allowEditing: false,
                    allowSelfIntersection: false,
                    allowSelfIntersectionEdit: false,
                    removeLayerBelowMinVertexCount: false,
                    allowRemoval: false,
                    allowCutting: false,
                    allowRotation: false,
                    snappable: false,
                });
            }
        }

    }, [controllerFunctionsActive, basementPolygonRef]);

    return <></>
}