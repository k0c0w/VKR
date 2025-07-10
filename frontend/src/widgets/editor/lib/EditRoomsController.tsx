import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, setBuildingStructureOnLevel } from "./planEditorSlice";
import { isRoom, isWall, RoomType, WallGeometry, RoomGeometry, Room, Wall } from "@entities/map";
import { getGeoJsonFeatureGeometryFrom, isKnownShapeLayer, isPolygonLayer, isPolylineLayer, toGeoJsonWithId } from "@shared/map/lib/leafletUtilsAdditions";
import { Layer, PM } from "leaflet";
import EnableOrDisableLayers from "./EnableOrDisableLayers";
import { generateRandomGuidWhichDoesNotExistsIn, generateRandomNumberWhichDoesNotExistsIn } from "@shared/utils/random";
import { castToLayerWithFeatureId, layerHasFeatureId, LayerWithFeatureId, mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { isAnyGeomanEditModeEnabled } from "./helpers";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION, SNAP_DISTANCE } from "@app/config/constants";
import { useBuildingMap } from "@shared/map";
import guid from "@shared/types/guid";
import { usePlanViewerContext } from "../ui/PlanViewerContext";
import { setDefaultStyle } from "@shared/map/lib/styling/styling";


const DEFAULT_ROOM_TYPE = RoomType.Audience;

function logUnsupportedShape(shape: PM.SUPPORTED_SHAPES) {
  console.warn("Creation of unsupported shape: %s.", shape);
}

function canNotRemoveLayerInEditMode(e: { layer: Layer; marker: L.Marker; event: any }): boolean {
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
  allowCutting: false,
  allowEditing: true,
  allowRotation: true,
  allowRemoval: true,
  draggable: true,
  allowSelfIntersection: false,
  snappable: true,
  snapDistance: SNAP_DISTANCE, 
  removeVertexValidation: canNotRemoveLayerInEditMode,
};

const whenDisabledOptions = {
  allowCutting: false,
  allowEditing: false,
  allowRotation: false,
  allowRemoval: false,
  draggable: false,
  allowSelfIntersection: false,
  removeVertexValidation: canNotRemoveLayerInEditMode,
};

export default function EditRoomsController() {
  const dispatch = useAppDispatch();
  const { currentStep, building } = useAppSelector((state) => state.planEditorSlice);
  const { map, buildingStructureLayerGroup, currentLevelIndex } = useBuildingMap();
  const { focusOn, unfocus } = usePlanViewerContext();

  if (!building) throw new Error("You must initialize slice first!");
  const levelFeatures = building.properties.levels[currentLevelIndex].buildingStructure;


  const setFeature = (featureId: guid | number, feature: Wall | Room | null) =>
    dispatch(setBuildingStructureOnLevel({ levelIndex: currentLevelIndex, featureId, feature }));

  function onLayerClicked(e: L.LeafletMouseEvent) {
    const layer = e.target as Layer;
    if (!(isPolygonLayer(layer) && layerHasFeatureId(layer))) return;
    if (isAnyGeomanEditModeEnabled(map.pm)) return;

    const feature = levelFeatures.find((x) => x.id === layer.featureId);
    if (!feature || !isRoom(feature)) return;

    focusOn(feature);
  }

  function updateFeaturePositionRelatedTo(layer: LayerWithFeatureId) {
    const oldFeature = levelFeatures.find((x) => x.id === layer.featureId);
    if (!oldFeature) return;

    const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);
    const updatedFeature = { ...oldFeature };
    if (isRoom(oldFeature) && geometry.type === "Polygon") {
      updatedFeature.geometry = geometry as RoomGeometry;
    } else if (isWall(oldFeature) && geometry.type === "LineString") {
      updatedFeature.geometry = geometry as WallGeometry;
    }
    setFeature(updatedFeature.id, updatedFeature as Wall | Room);
  }

  function handleCreate({ shape, layer }: { shape: PM.SUPPORTED_SHAPES; layer: Layer }) {
    if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup || !isKnownShapeLayer(layer)) return;

    layer.pm.setOptions({ ...whenEnabledOptions });
    setDefaultStyle(layer);
    const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);
    let feature: Wall | Room;
    let featureId: guid | number;

    switch (shape) {
      case "Line":
        featureId = generateRandomGuidWhichDoesNotExistsIn(Object.keys(levelFeatures));
        feature = {
          type: "Feature",
          id: featureId,
          geometry: geometry as WallGeometry,
          properties: { meaning: "Wall" },
        };
        break;
      case "Polygon":
        featureId = generateRandomNumberWhichDoesNotExistsIn(Object.keys(levelFeatures).filter(x => typeof x === "number").map(x => Number(x)));
        feature = {
          type: "Feature",
          id: featureId,
          geometry: geometry as RoomGeometry,
          properties: {
            meaning: "Room",
            type: DEFAULT_ROOM_TYPE,
            name: "",
          },
        };
        break;
      default:
        logUnsupportedShape(shape);
        return;
    }

    setFeature(featureId, feature);

    const workingLayer = mutateToLayerWithFeatureIdBasedOn(layer, featureId);
    setFeature(featureId, feature);
    workingLayer.addTo(buildingStructureLayerGroup);
    layer.remove();

    if (isRoom(feature)) focusOn(feature);
  }

  function handleRotateEnd({ layer }: { layer: L.Layer }) {
    if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup || !isKnownShapeLayer(layer)) return;
    const workingLayer = castToLayerWithFeatureId(layer);
    updateFeaturePositionRelatedTo(workingLayer);
  }

  function handleRemove({ layer }: { layer: L.Layer; shape: PM.SUPPORTED_SHAPES }) {
    if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup || !isKnownShapeLayer(layer)) return;
    const workingLayer = castToLayerWithFeatureId(layer);
    setFeature(workingLayer.featureId, null);
    unfocus();
  }

  function handleLayerVerticesChange({ layer }: { layer: Layer }) {
    if (currentStep !== CreateNewPlanStep.RoomsBoundariesSetup || !isKnownShapeLayer(layer)) return;
    const workingLayer = castToLayerWithFeatureId(layer);
    updateFeaturePositionRelatedTo(workingLayer);
  }

  function handleVertexDragEnd({
    layer,
    intersectionReset,
  }: {
    layer: L.Layer;
    indexPath: number;
    markerEvent: any;
    shape: PM.SUPPORTED_SHAPES;
    intersectionReset: boolean;
  }) {
    if (
      currentStep !== CreateNewPlanStep.RoomsBoundariesSetup ||
      !isKnownShapeLayer(layer) ||
      intersectionReset
    )
      return;
    const workingLayer = castToLayerWithFeatureId(layer);
    updateFeaturePositionRelatedTo(workingLayer);
  }

  useEffect(() => {
    if (currentStep === CreateNewPlanStep.RoomsBoundariesSetup) {
      map.on("pm:drawstart", ({ shape, workingLayer }) => {
        // @ts-ignore
        map.pm.Draw[shape].setOptions({ ...whenEnabledOptions });
        setDefaultStyle(workingLayer);
      });
      map.on("pm:create", handleCreate);
      buildingStructureLayerGroup.eachLayer((layer: Layer) => {
        layer.on("pm:remove", handleRemove);
        layer.on("pm:rotateend", handleRotateEnd);
        layer.on("pm:dragend", handleLayerVerticesChange);
        layer.on("pm:vertexadded", handleLayerVerticesChange);
        layer.on("pm:vertexremoved", handleLayerVerticesChange);
        layer.on("pm:markerdragend", handleVertexDragEnd);
      });
    }

    return () => {
      map.off("pm:drawstart");
      map.off("pm:create", handleCreate);
      buildingStructureLayerGroup.eachLayer((layer: Layer) => {
        layer.off("pm:remove", handleRemove);
        layer.off("pm:rotateend", handleRotateEnd);
        layer.off("pm:dragend", handleLayerVerticesChange);
        layer.off("pm:vertexadded", handleLayerVerticesChange);
        layer.off("pm:vertexremoved", handleLayerVerticesChange);
        layer.off("pm:markerdragend", handleVertexDragEnd);
      });
    };
  }, [
    map,
    currentStep,
    buildingStructureLayerGroup,
    handleCreate,
    handleRemove,
    handleRotateEnd,
    handleLayerVerticesChange,
    handleVertexDragEnd,
  ]);

  useEffect(() => {
    const layers = buildingStructureLayerGroup.getLayers()
      .filter(x => layerHasFeatureId(x)) as LayerWithFeatureId[];

    layers.forEach(x => {
      x.on("click", onLayerClicked);
    });

    return () => {
      layers.forEach(x => x.off("click", onLayerClicked));
    };
  }, [levelFeatures, onLayerClicked]);

  return (
    <EnableOrDisableLayers
      enabled={currentStep === CreateNewPlanStep.RoomsBoundariesSetup}
      layers={buildingStructureLayerGroup.getLayers() as LayerWithFeatureId[]}
      whenDisabledOptions={whenDisabledOptions}
      whenEnabledOptions={whenEnabledOptions}
    />
  );
}