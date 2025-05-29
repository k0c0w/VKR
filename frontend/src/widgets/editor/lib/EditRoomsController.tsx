import { useEffect, useState } from "react"
import { useMap } from "react-leaflet";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, focusOnFeature, setBuildingStructureOnCurrentLevel } from "./planEditorSlice";
import { LatLng, Layer, Marker, PM, Polygon, Polyline } from "leaflet";
import {  isRoom, isWall, RoomType, WallGoometry, RoomGeometry, Room, Wall, FeatureWithId } from "@entities/map";
import { getGeoJsonFeatureGeometryFrom, getLayerLeafletId, isKnownShapeLayer, isMarkerLayer, isPolygonLayer, isPolylineLayer, toGeoJsonWithId, } from "@shared/map/lib/leafletUtilsAdditions";
import {GeoJSON} from "leaflet";
import EnableOrDisableLayers from "./EnableOrDisableLayers";
import { guid, } from "@shared/types/guid";
import { generateRandomGuidWhichDoesNotExistsIn } from "@shared/utils/random";
import { isPoint } from "@shared/types/geoJsonTypeGuards";
import L from "leaflet";
import { castToLayerWithFeatureId, layerHasFeatureId, LayerWithFeatureId, mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { errorStyle, wallStyle } from "./styling/styling";
import { getStyleByRoomType, setDefaultStyle } from "./geoman/utils";
import { isValidFeaturePosition } from "./layerValidation";
import { LineString, Polygon as GeoJsonPolygon, Point } from "geojson";
import { resetStyle, splitWallsAndRooms } from "./helpers";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { BuildingMapPanes } from "@shared/map";

const DEFAULT_ROOM_TYPE = RoomType.Audience;

function logUnsupportedShape(shape: PM.SUPPORTED_SHAPES) {
    console.warn("Creation of unsupported shape: %s.", shape);
}

function canNotRemoveLayerInEditMode(e: {
      layer: Layer;
      marker: L.Marker;
      event: any;
    }):boolean {
    const layer = e.layer;
    if (isPolygonLayer(layer)) {
        const latLngs = layer.getLatLngs();
        // @ts-ignore
        return latLngs.length >= 1 && latLngs[0].length && latLngs[0].length > 3;
    } else if (isPolylineLayer(layer)) {
        return layer.getLatLngs().length > 2;
    }
    
    return true;
}

const whenEnabledOptions = {
    allowCutting: true,
    allowEditing: true,
    allowRotation: true,
    allowRemoval: true,
    draggable: true,
    allowSelfIntersection: false,
    removeVertexValidation: canNotRemoveLayerInEditMode,
}

const whenDisabledOptions = {
    allowCutting: false,
    allowEditing: false,
    allowRotation: false,
    allowRemoval: false,
    draggable: false,
    allowSelfIntersection: false,
    removeVertexValidation: canNotRemoveLayerInEditMode,
}

export default function EditRoomsController() {
    const dispatch = useAppDispatch();
    const { currentStep, currentLevelIndex, building } = useAppSelector(state => state.planEditorSlice);

    if (!building) {
        throw new Error("You must initialize slice first!");
    }

    const [lastLevelRender, setLastLevelRender] = useState(`${currentLevelIndex}:${building.properties.levels.length}`);
    const levelFeatures = building!.properties.levels[currentLevelIndex].buildingStructure;
    const map = useMap();

    const [layers, setLayers] = useState<{[layerId: number]: LayerWithFeatureId}>( {});
    const setLayer = (layer: LayerWithFeatureId, options?: {delete: boolean}) => {
        setLayers(prev => {
            const updatedLayers = {...prev};
    
            if (options && options.delete) {
                delete updatedLayers[getLayerLeafletId(layer)];
            } else {
                updatedLayers[getLayerLeafletId(layer)] = layer;
            }
    
            return updatedLayers;
        });
    } 
    const setFeature = (featureId: guid, feature: Wall | Room | null) => dispatch(setBuildingStructureOnCurrentLevel({featureId, feature}));
    const focusOn = ({id}: Room) => dispatch(focusOnFeature({featureId:id, levelIndex: currentLevelIndex, isITInfrastructureFeature: false}));
    const {walls, rooms} = splitWallsAndRooms(layers);

    function onLayerClicked(e: L.LeafletMouseEvent) {
        const layer = castToLayerWithFeatureId(e.target as Layer);
        if (!isPolygonLayer(layer)) {
            return;
        }

        const feature = levelFeatures.find(x => x.id === layer.featureId);
        if (!feature || !isRoom(feature)) {
            return;
        }
        
        focusOn(feature);
    }

    function updateFeaturePositionRelatedTo(layer: LayerWithFeatureId) {
        const oldFeature = levelFeatures.find(x => x.id === layer.featureId);
        if (oldFeature === undefined) {
            return;
        }

        const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);
        
        const updatedFeature = {...oldFeature};
        if (isRoom(oldFeature) && geometry.type === "Polygon") {
            updatedFeature.geometry = geometry as RoomGeometry;
        } else if (isWall(oldFeature) && geometry.type === "LineString") {
            updatedFeature.geometry = geometry as WallGoometry;
        }
        setFeature(updatedFeature.id, updatedFeature as Wall | Room);
    }

    function handleCreate({shape, layer}: { shape: PM.SUPPORTED_SHAPES; layer: Layer }) {
        if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup) {
            return;
        }

        if (!isKnownShapeLayer(layer)){
            return;
        }
        layer.pm.setOptions({
            ...whenEnabledOptions,
        });
        setDefaultStyle(layer);
        const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);

        const featureId = generateRandomGuidWhichDoesNotExistsIn(Object.keys(levelFeatures));
        const workingLayer = mutateToLayerWithFeatureIdBasedOn(layer, featureId);

        let feature: Wall | Room;
        switch(shape) {
            case "Line":
                feature = {
                    type: "Feature",
                    id: featureId,
                    geometry: geometry as WallGoometry,
                    properties: {
                        meaning: "Wall"
                    }
                }
                break;

            case "Polygon":
                feature = {
                    type: "Feature",
                    id: featureId,
                    geometry: geometry as RoomGeometry,
                    properties: {
                        meaning: "Room",
                        type: DEFAULT_ROOM_TYPE,
                    }
                }
                break;

            default:
                logUnsupportedShape(shape);
                return;
        }

        setFeature(featureId, feature);
        setLayer(workingLayer);

        if (isRoom(feature)) {
            focusOn(feature);
        }
    }

    function handleRotateEnd(e: {
         layer: L.Layer;
         helpLayer: L.Layer;
         startAngle: number;
         angle: number;
         originLatLngs: L.LatLng[];
         newLatLngs: L.LatLng[];
     }) {
        const { layer, } = e;
        
        if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup || !isKnownShapeLayer(layer)) {
            return;
        }

        const workingLayer = castToLayerWithFeatureId(layer);
        updateFeaturePositionRelatedTo(workingLayer);
    };
   
    function handleRemove({layer}: {
        layer: L.Layer;
        shape: PM.SUPPORTED_SHAPES;
    }) {
        if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup) {
            return;
        } else if (!isKnownShapeLayer(layer)) {
            return;
        }

        const workingLayer = castToLayerWithFeatureId(layer);
        
        setFeature(workingLayer.featureId, null);
        setLayer(workingLayer, { delete: true });
    }
    
    function handleLayerVerticesChange({layer}: {layer: Layer}) {
        if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup) {
            return;
        } else if (!isKnownShapeLayer(layer)) {
            return;
        }

        const workingLayer = castToLayerWithFeatureId(layer);
        updateFeaturePositionRelatedTo(workingLayer);
    }
    
    function handleVertexDragEnd({layer, indexPath, markerEvent, intersectionReset}: {
        layer: L.Layer;
        indexPath: number;
        markerEvent: any;
        shape: PM.SUPPORTED_SHAPES;
        intersectionReset: boolean;
    }) {
       if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup) {
           return;
       } else if (!isKnownShapeLayer(layer)) {
           return;
       } else if (intersectionReset) {
           return;
       }

        const workingLayer = castToLayerWithFeatureId(layer);
        updateFeaturePositionRelatedTo(workingLayer);
    }

    function setErrorStyleOrResetStyleForExistingLayer({layer}: {layer: Layer}) {
        if (layerHasFeatureId(layer)) {
            const feature = levelFeatures.find(x => x.id === layer.featureId);
            if (!feature || isPoint(feature)) {
                return;
            }

            const newPosition = toGeoJsonWithId(layer);

            if (isValidFeaturePosition({feature: newPosition, bounds: building!.geometry, walls, rooms })) {
                resetStyle(layer, feature);
            } else {
                // @ts-ignore
                layer.setStyle(errorStyle);
            }
        }
    }

    useEffect(() => {
        if (!map.levelControl) {
            return;
        } else {
            map.levelControl.setShowLevelButtons(currentStep === CreateNewPlanStep.RoomsBoundariesSetup);
        }
    }, [map, currentStep]);

    useEffect(() => {
        Object.values(layers).forEach(layer => {
            layer.on("pm:remove", handleRemove);
            layer.on("pm:rotateend", handleRotateEnd);
            layer.on("pm:dragend", handleLayerVerticesChange);
            layer.on("pm:vertexadded", handleLayerVerticesChange);
            layer.on("pm:vertexremoved", handleLayerVerticesChange);
            layer.on("pm:markerdragend", handleVertexDragEnd);

            layer.on("pm:rotate", setErrorStyleOrResetStyleForExistingLayer);
            layer.on("pm:drag", setErrorStyleOrResetStyleForExistingLayer);
            layer.on("pm:markerdrag", setErrorStyleOrResetStyleForExistingLayer);
        });

        return () => {
            Object.values(layers).forEach(layer => {
                layer.off("pm:remove", handleRemove);
                layer.off("pm:rotateend", handleRotateEnd);
                layer.off("pm:dragend", handleLayerVerticesChange);
                layer.off("pm:vertexadded", handleLayerVerticesChange);
                layer.off("pm:vertexremoved", handleLayerVerticesChange);
                layer.off("pm:markerdragend", handleVertexDragEnd);

                layer.off("pm:drag", setErrorStyleOrResetStyleForExistingLayer);
                layer.off("pm:rotate", setErrorStyleOrResetStyleForExistingLayer);
                layer.off("pm:markerdragend", setErrorStyleOrResetStyleForExistingLayer);
            });
        }
    }, [layers, handleRemove, handleLayerVerticesChange, handleRotateEnd, handleRotateEnd, setErrorStyleOrResetStyleForExistingLayer]);

    useEffect(() => {
        const setupDraw: PM.DrawStartEventHandler = function ({shape, workingLayer}) {
            // @ts-ignore
            map.pm.Draw[shape].setOptions({
                ...whenEnabledOptions,
            });

            setDefaultStyle(workingLayer);
        }

        if (currentStep === CreateNewPlanStep.RoomsBoundariesSetup) {
            map.on("pm:drawstart", setupDraw);
        }

        return () => {
            map.off("pm:drawstart", setupDraw);
        }
    }, [map, currentStep]);

    useEffect(() => {
        if (currentStep === CreateNewPlanStep.RoomsBoundariesSetup) {
            map.on("pm:create", handleCreate);
        }

        return () => { map.off("pm:create", handleCreate) }
    }, [map, currentStep, handleCreate]);

    useEffect(() => {
        const thisRenderRequest = `${currentLevelIndex}:${building.properties.levels.length}:${building.properties.levels[currentLevelIndex].buildingStructure.length}`

        if (lastLevelRender !== thisRenderRequest) {
            for (const layerId in layers) {
                layers[layerId].remove();
            }

            const levelFeatures = building.properties.levels[currentLevelIndex].buildingStructure;
            const newLeafletId2FeatureId: { [layerId: number]: guid } = {};
            const newLayers: { [layerId: number]: LayerWithFeatureId } = {};

            const paneOptions = { pane: BuildingMapPanes.buildingStructure.pane }
            for (const feature of levelFeatures) {
                let newLayer: Polyline | Polygon | Marker;
                if (isWall(feature)) {
                    const coords = feature.geometry.coordinates.map(position =>
                        GeoJSON.coordsToLatLng(position as [number, number])
                    ) as LatLng[];
                    newLayer = L.polyline(coords, paneOptions) as Polyline;
                    newLayer.setStyle(wallStyle);
                } else if (isRoom(feature)) {
                    const latlngs = feature.geometry.coordinates.map(positions =>
                        GeoJSON.coordsToLatLngs(positions)
                    ) as LatLng[][];
                    newLayer = new Polygon(latlngs, paneOptions);
                    newLayer.setStyle(getStyleByRoomType(feature.properties.type));
                } else if (isPoint(feature)) {
                    alert("Point!!");
                    continue;
                } else {
                    continue;
                }

                newLayer.pm.setOptions({
                    ...whenEnabledOptions,
                });

                newLayer.addTo(map);

                const layerId = getLayerLeafletId(newLayer);
                newLayers[layerId] = mutateToLayerWithFeatureIdBasedOn(newLayer, feature.id);
                newLeafletId2FeatureId[layerId] = feature.id;
            }

            setLastLevelRender(thisRenderRequest);
            setLayers(newLayers);
        }
    }, [map, currentLevelIndex, lastLevelRender, layers, building, setLastLevelRender, setLayers]);

    useEffect(() => {
        Object.values(layers).forEach(layer => {
            layer.off("click");
            layer.on("click", onLayerClicked)
        }
    )}, [layers, levelFeatures, onLayerClicked]);

    useEffect(() => {
        const validationArgs: {feature: FeatureWithId<LineString | GeoJsonPolygon | Point> | null; bounds: GeoJsonPolygon; rooms:FeatureWithId<RoomGeometry>[]; walls: FeatureWithId<WallGoometry>[]}
         = {
            bounds: building.geometry,
            rooms,
            walls,
            feature: null
        };     

        Object.values(layers).forEach(layer => {
            if (isKnownShapeLayer(layer)) {
                const feature = levelFeatures.find(x => x.id === layer.featureId);
                if (!feature) {
                    return;
                }
                validationArgs.feature = feature;

                if (!isValidFeaturePosition(validationArgs as {feature: FeatureWithId<LineString | GeoJsonPolygon | Point>; bounds: GeoJsonPolygon; walls:FeatureWithId<WallGoometry>[]; rooms: FeatureWithId<RoomGeometry>[]})) {
                    if (!isMarkerLayer(layer)) {
                        layer.setStyle(errorStyle);
                    }
                } else {
                    resetStyle(layer, feature);
                }
            }
        });
    }, [layers, building]);

    return <>
        <EnableOrDisableLayers enabled={currentStep === CreateNewPlanStep.RoomsBoundariesSetup} 
            layers={Object.values(layers)}
            whenDisabledOptions={whenDisabledOptions}
            whenEnabledOptions={whenEnabledOptions}
        />
    </>
}