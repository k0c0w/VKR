import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { useEffect, useState } from "react"
import { useMap } from "react-leaflet";
import { CreateNewPlanStep, focusOnFeature, setItInfrastructureOnCurrentLevel } from "./planEditorSlice";
import EnableOrDisableLayers from "./EnableOrDisableLayers";
import { GeoJSON, Layer, Marker, PM } from "leaflet";
import { getGeoJsonFeatureGeometryFrom, getLayerLeafletId, isMarkerLayer } from "@shared/map";
import { BuildingGeometry, FeatureWithId, isWall, ITInfrastructure, ITInfrastructureGeometry, RoomGeometry, Wall, WallGoometry } from "@entities/map";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { guid } from "@shared/types/guid";
import { generateRandomGuidWhichDoesNotExistsIn } from "@shared/utils/random";
import { layerHasFeatureId, LayerWithFeatureId, mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { isValidFeaturePosition } from "./layerValidation";
import { Point, Polygon as GeoJsonPolygon } from "geojson";
import { errorItInfrastructureIcon, itInfrastructureIcon } from "./styling/styling";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";

const whenEnabledOptions = {
    allowCutting: false,
    allowEditing: false,
    allowRotation: false,
    allowSelfIntersection: false,
    allowRemoval: true,
    draggable: true,
    snappable: true,
}

const whenDisabledOptions = {
    allowCutting: false,
    allowEditing: false,
    allowRotation: false,
    allowSelfIntersection: false,
    allowRemoval: false,
    draggable: false,
    snappable: false,
}

export default function EditItInfrastructureController() {
    const dispatch = useAppDispatch();
    const { currentStep, currentLevelIndex, building } = useAppSelector(state => state.planEditorSlice);
    if (!building) {
        throw new Error("You must initialize slice first!");
    }

    const [lastLevelRender, setLastLevelRender] = useState(`${currentLevelIndex}:${building.properties.levels.length}`);

    const level = building!.properties.levels[currentLevelIndex];
    const levelFeatures = level.infrastructure;
    const walls = level.buildingStructure.filter(x => isWall(x)) as Wall[];

    const [layers, setLayers] = useState<{[layerId: number]: LayerWithFeatureId}>( {});
    const setFeature = (featureId: guid, feature: ITInfrastructure | null) =>  dispatch(setItInfrastructureOnCurrentLevel({featureId,feature}));
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
    const map = useMap();
    const focusOn = ({id}: ITInfrastructure) => dispatch(focusOnFeature({featureId:id, levelIndex: currentLevelIndex, isITInfrastructureFeature: true}));

    function onMarkerClick(feature: ITInfrastructure) {
        if (!(map.pm.globalRemovalModeEnabled() || map.pm.globalDragModeEnabled())) {
            focusOn(feature);
        }
    }

    function handleCreate({shape, layer}: { shape: PM.SUPPORTED_SHAPES; layer: Layer }) {
        if (currentStep !== CreateNewPlanStep.InfrastructureSetup) {
            return;
        } else if (!isMarkerLayer(layer)) {
            return;
        } else if(shape !== "Marker") {
            console.warn("Creation of unsupported shape: %s.", shape);
            return;
        }

        layer.pm.setOptions({
            ...whenEnabledOptions,
        });
        const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);
        const featureId = generateRandomGuidWhichDoesNotExistsIn(Object.keys(levelFeatures));

        const feature: ITInfrastructure = {
            type: "Feature",
            id: featureId,
            geometry: geometry as ITInfrastructureGeometry,
            properties: {
                meaning: "IT-Infrastructure",
                inventoryNumber: "",
                name: "",
                linkedToAudienceId: "",
                serialNumber: ""
            }
        };

        layer.on("click", () => onMarkerClick(feature));

        setFeature(featureId, feature);
        setLayer(mutateToLayerWithFeatureIdBasedOn(layer, featureId));
    }

    function handlePositionChange({layer}: {
      layer: L.Layer;
      shape: PM.SUPPORTED_SHAPES;
    }) {
        if (currentStep !== CreateNewPlanStep.InfrastructureSetup) {
            return;
        } else if (!isMarkerLayer(layer) || !layerHasFeatureId(layer)) {
            return;
        }
        
        const prevState = levelFeatures.find(x => x.id === layer.featureId);
        if (!prevState) {
            return;
        }

        const rawFeature = layer.toGeoJSON(GEOJSON_PRECISION);
        const roundedGeometry = roundCoordinates(rawFeature.geometry, GEOJSON_PRECISION);
        setFeature(layer.featureId, {...prevState, geometry: roundedGeometry});
    }

    function setErrorStyleIfInvalid({layer}: {layer: Layer}) {
        if (!isMarkerLayer(layer)) {
            return;
        }

        const feature: FeatureWithId<Point> = {...layer.toGeoJSON(GEOJSON_PRECISION), id: getLayerLeafletId(layer).toString()};

        if(!isValidFeaturePosition({
            bounds: building!.geometry,
            walls,
            rooms: [],
            feature
        })) {
            if (layer.getIcon() !== errorItInfrastructureIcon) {
                layer.setIcon(errorItInfrastructureIcon);
            }
        } else {
            if (layer.getIcon() !== itInfrastructureIcon ) {
                layer.setIcon(itInfrastructureIcon);
            }
        }
    }

    function handleRemove({layer}: {
        layer: L.Layer;
        shape: PM.SUPPORTED_SHAPES;
    }) {
        if (currentStep !== CreateNewPlanStep.InfrastructureSetup) {
            return;
        } else if (!isMarkerLayer(layer) || !layerHasFeatureId(layer)) {
            return;
        }
        
        setFeature(layer.featureId, null);
        setLayer(layer, { delete: true });
    }

    useEffect(() => {
        Object.values(layers).forEach(layer => {
            layer.on("pm:remove", handleRemove);
            layer.on("pm:drag", setErrorStyleIfInvalid);
            layer.on("pm:dragend", handlePositionChange);
        });

        return () => {
            Object.values(layers).forEach(layer => {
                layer.off("pm:remove", handleRemove);
                layer.off("pm:dragend", handlePositionChange);
                layer.off("pm:drag", setErrorStyleIfInvalid);
            });
        }
    }, [layers, building, handleRemove, handlePositionChange, setErrorStyleIfInvalid]);

    useEffect(() => {
        if (currentStep === CreateNewPlanStep.InfrastructureSetup) {
            map.on("pm:create", handleCreate);
        }

        return () => { map.off("pm:create", handleCreate) }
    }, [map, currentStep, handleCreate]);

    useEffect(() => {
        const setupDraw: PM.DrawStartEventHandler = function ({shape, workingLayer}) {
            console.log(shape);
            //@ts-ignore
            map.pm.Draw.Marker.setOptions({
                ...whenEnabledOptions,
                markerStyle: {
                    icon: itInfrastructureIcon,
                    opacity: 1,
                }
            })
        }

        if (currentStep === CreateNewPlanStep.InfrastructureSetup) {
            map.on("pm:drawstart", setupDraw);
        }

        return () => {
            map.off("pm:drawstart", setupDraw);
        }
    }, [map, currentStep]);

    useEffect(() => {
        const thisRenderRequest = `${currentLevelIndex}:${building.properties.levels.length}`

        if (lastLevelRender !== thisRenderRequest) {
            for(const layerId in layers) {
                layers[layerId].remove();
            }

            const levelFeatures = building.properties.levels[currentLevelIndex].infrastructure;
            
            const newLayers: {[layerId: number]: LayerWithFeatureId}  = {};
            for (const feature of levelFeatures) {
                const newLayer: Marker = new Marker(GeoJSON.coordsToLatLng(feature.geometry.coordinates as [number, number]));

                newLayer.pm.setOptions({
                    ...whenEnabledOptions,
                });

                newLayer.on("click", () => onMarkerClick(feature));
                newLayer.addTo(map);

                const workingLayer = mutateToLayerWithFeatureIdBasedOn(newLayer, feature.id);

                newLayers[getLayerLeafletId(workingLayer)] = workingLayer;
            }

            setLastLevelRender(thisRenderRequest);
            setLayers(newLayers);
        }

    }, [map, currentLevelIndex, lastLevelRender, layers, building, setLastLevelRender, setLayers]);

    useEffect(() => {
        const validationArgs: {feature: FeatureWithId<Point> | null; bounds: GeoJsonPolygon; walls:FeatureWithId<WallGoometry>[]; rooms: FeatureWithId<RoomGeometry>[]}
         = {
            bounds: building.geometry,
            rooms: [],
            walls,
            feature: null
        };     

        Object.values(layers).forEach(layer => {
            if (isMarkerLayer(layer)) {
                const feature = levelFeatures.find(x => x.id === layer.featureId);
                if (!feature) {
                    return;
                }
                validationArgs.feature = feature;

                if (!isValidFeaturePosition(validationArgs as {feature: FeatureWithId<Point>; bounds: BuildingGeometry; walls:FeatureWithId<WallGoometry>[]; rooms: FeatureWithId<RoomGeometry>[]})) {
                    layer.setIcon(errorItInfrastructureIcon);
                } else {
                    layer.setIcon(itInfrastructureIcon);
                }
            }
        });
    }, [layers, levelFeatures, building]);

    return <>
        <EnableOrDisableLayers 
            enabled={currentStep === CreateNewPlanStep.InfrastructureSetup}
            layers={Object.values(layers)} 
            whenDisabledOptions={whenDisabledOptions} 
            whenEnabledOptions={whenEnabledOptions} />
    </>
}