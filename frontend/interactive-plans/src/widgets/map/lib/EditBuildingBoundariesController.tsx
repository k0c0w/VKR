import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks"
import { CreateNewPlanStep, setBuildingBounds } from "./createNewPlanSlice";
import BuildingBasePolygon from "./BuildingBasePolygon";
import { useEffect, useRef, useState } from "react";
import { LatLngLiteral, Polygon } from "leaflet";
import { useMap } from "react-leaflet";
import {setUpControls} from "./geoman/utils";

/* TODO: добавить возможность вырезать полости из полигона */
export default function EditBuildingBoundariesController({initialBoundaries}: {initialBoundaries: LatLngLiteral[][]}) {
    const dispatch = useAppDispatch();
    const [hasBeenFocusedOnInit, setHasBeenFocusedOnInit] = useState(false);
    const ref = useRef<Polygon| null>(null);
    const { currentStep } = useAppSelector(state => state.createNewPlanReducer);
    const map = useMap();

    function handleShapeChange(geometry: LatLngLiteral[][]) {
        dispatch(setBuildingBounds(geometry));
    }

    useEffect(() => {
        const layer = ref.current;
        if (layer && !hasBeenFocusedOnInit) {
            const bounds = layer.getBounds();
            map.fitBounds(bounds);
            setHasBeenFocusedOnInit(true);
        }
    }, [ref, hasBeenFocusedOnInit, map]);

    useEffect(() => {
        if (currentStep === CreateNewPlanStep.BuildingBoundariesSetup) {
            const pm = map.pm;

            setUpControls(pm, {
                controlsVisable: false,
                cutMode: false,
                dragMode: false,
                drawMode: false,
                removalMode: false,
                rotationMode: false,
                editMode: false,
            });

            ref.current?.pm.enable({
                allowSelfIntersectionEdit: false,
                allowSelfIntersection: false
            });
        }

    }, [currentStep, map, ref]);

    useEffect(() => {
        dispatch(setBuildingBounds(initialBoundaries));
    }, [initialBoundaries, dispatch])

    return <BuildingBasePolygon
        ref={ref}
        positions={initialBoundaries}
        editable={currentStep === CreateNewPlanStep.BuildingBoundariesSetup}
        onChange={handleShapeChange}
    />
}