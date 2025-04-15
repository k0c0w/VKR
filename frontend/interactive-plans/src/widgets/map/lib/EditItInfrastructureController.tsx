import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { useEffect, useState } from "react"
import { useMap } from "react-leaflet";
import { CreateNewPlanStep, focusOnFeature, setItInfrastructureOnCurrentLevel } from "./createNewPlanSlice";
import EnableOrDisableLayers from "./EnableOrDisableLayers";
import { GeoJSON, Layer, Marker, PM } from "leaflet";
import { getGeoJsonFeatureGeometryFrom, getLayerLeafletId, isMarkerLayer } from "@shared/map";
import { FeatureWithId, isWall, ITInfrastructure, ITInfrastructureGeometry, Wall } from "@entities/map";
import { TO_GEOJSON_PRECISION } from "@app/config/constants";
import { guid } from "@shared/types/guid";
import { generateRandomGuidWhichDoesNotExistsIn } from "@shared/utils/random";
import { layerHasFeatureId, LayerWithFeatureId, mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { isValidFeaturePosition } from "./layerValidation";
import { LineString, Point, Polygon as GeoJsonPolygon } from "geojson";

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
    const { currentStep, currentLevelIndex, building } = useAppSelector(state => state.createNewPlanReducer);
    if (!building) {
        throw new Error("You must initialize slice first!");
    }

    const [lastLevelRender, setLastLevelRender] = useState(`${currentLevelIndex}:${building.properties.levels.length}`);

    const level = building!.properties.levels[currentLevelIndex];
    const levelFeatures = level.infrastructure;
    const walls = level.buildingStructure.filter(x => isWall(x)) as Wall[];

    const [layers, setLayers] = useState<{[layerId: number]: LayerWithFeatureId}>({});
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
        const geometry = getGeoJsonFeatureGeometryFrom(layer);
        const featureId = generateRandomGuidWhichDoesNotExistsIn(Object.keys(levelFeatures));

        const feature: ITInfrastructure = {
            type: "Feature",
            id: featureId,
            geometry: geometry as ITInfrastructureGeometry,
            properties: {
                meaning: "IT-Infrastructure",
                inventoryNumber: "",
                name: "",
                serialNumber: ""
            }
        };

        layer.on("click", () => focusOn(feature));

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

        const rawFeature = layer.toGeoJSON(TO_GEOJSON_PRECISION);
        setFeature(layer.featureId, {...prevState, geometry: rawFeature.geometry});
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

    // Update validator for layers
    useEffect(() => {
        Object.values(layers).forEach(layer => {
            layer.on("pm:remove", handleRemove);
            layer.on("pm:dragend", handlePositionChange);
        });

        return () => {
            Object.values(layers).forEach(layer => {
                layer.off("pm:remove", handleRemove);
                layer.off("pm:dragend", handlePositionChange);
            });
        }
    }, [layers, building, handleRemove, handlePositionChange]);

    // Handle layer creation
    useEffect(() => {
        if (currentStep === CreateNewPlanStep.InfrastructureSetup) {
            map.on("pm:create", handleCreate);
        }

        return () => { map.off("pm:create", handleCreate) }
    }, [map, currentStep, handleCreate]);

    // Add validation on draw start
    useEffect(() => {
            const setupDraw: PM.DrawStartEventHandler = function ({shape}) {
                // @ts-ignore
                map.pm.Draw[shape].setOptions({
                    ...whenEnabledOptions,
                });
            }
    
            if (currentStep === CreateNewPlanStep.InfrastructureSetup) {
                map.on("pm:drawstart", setupDraw);
            }
    
            return () => {
                map.off("pm:drawstart", setupDraw);
            }
    }, [map, currentStep]);

    // Level change handling
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

                newLayer.addTo(map);

                const workingLayer = mutateToLayerWithFeatureIdBasedOn(newLayer, feature.id);

                newLayers[getLayerLeafletId(workingLayer)] = workingLayer;
            }

            setLastLevelRender(thisRenderRequest);
            setLayers(newLayers);
        }

    }, [map, currentLevelIndex, lastLevelRender, layers, building, setLastLevelRender, setLayers]);

    // validate layers
    useEffect(() => {
        const validationArgs: {feature: FeatureWithId<Point> | null; bounds: GeoJsonPolygon; lineStrings:FeatureWithId<LineString>[]; polygons: FeatureWithId<GeoJsonPolygon>[]}
         = {
            bounds: building.geometry,
            polygons: [],
            lineStrings: walls,
            feature: null
        };     

        Object.values(layers).forEach(layer => {
            if (isMarkerLayer(layer)) {
                const feature = levelFeatures.find(x => x.id === layer.featureId);
                if (!feature) {
                    return;
                }
                validationArgs.feature = feature;

                if (!isValidFeaturePosition(validationArgs as {feature: FeatureWithId<Point>; bounds: GeoJsonPolygon; lineStrings:FeatureWithId<LineString>[]; polygons: FeatureWithId<GeoJsonPolygon>[]})) {
                    if (isMarkerLayer(layer)) {
                        // todo: set error icon here
                    }
                } else {
                    // todo: reset error icon here
                }
            }
        });
    }, [layers, building]);

    return <>
        <EnableOrDisableLayers 
            enabled={currentStep === CreateNewPlanStep.InfrastructureSetup}
            layers={Object.values(layers)} 
            whenDisabledOptions={whenDisabledOptions} 
            whenEnabledOptions={whenEnabledOptions} />
    </>
}
