import { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '@shared/hooks/reduxTypedHooks';
import { CreateNewPlanStep, setItInfrastructureOnLevel } from './planEditorSlice';
import EnableOrDisableLayers from './EnableOrDisableLayers';
import { Layer, PM } from 'leaflet';
import { getGeoJsonFeatureGeometryFrom, isMarkerLayer, MapStyling, roundCoordinates, useBuildingMap } from '@shared/map';
import { isRoom, Room, RoomId } from '@entities/map';
import { GEOJSON_PRECISION } from '@app/config/constants';
import { layerHasFeatureId, LayerWithFeatureId, mutateToLayerWithFeatureIdBasedOn } from '@shared/map/lib/leafletTypeExtensions';
import { isITEquipment, isITEquipmentInsideRoom, ITEquipment, ITEquipmentGeometry, ItEquipmentId } from '@entities/map/models/ItEquipment';
import { CatalogueItEquipment } from '@entities/catalogue';
import { usePlanViewerContext } from '../ui/PlanViewerContext';
import { isAnyGeomanEditModeEnabled } from './helpers';
import ItEquipmentDialog from '../ui/SelectItEquipmentFromCatalogueDialog';

const whenEnabledOptions = {
  allowCutting: false,
  allowEditing: false,
  allowRotation: false,
  allowSelfIntersection: false,
  allowRemoval: true,
  draggable: true,
  snappable: true,
  continueDrawing: true,
};

const whenDisabledOptions = {
  allowCutting: false,
  allowEditing: false,
  allowRotation: false,
  allowSelfIntersection: false,
  allowRemoval: false,
  draggable: false,
  snappable: false,
};

export default function EditItEquipmentController() {
  const [markerDrawModeEnabled, setMarkerDrawModeEnabled] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [pendingEquipment, setPendingEquipment] = useState<{ equipment: CatalogueItEquipment; roomId: number } | null>(null);

  const dispatch = useAppDispatch();
  const { currentStep, building, buildingInfoFromCatalogue } = useAppSelector((state) => state.planEditorSlice);
  const { itInfrastructureLayerGroup, map, currentLevelIndex, highlightRoom, unhighlightRoom } = useBuildingMap();
  const { focusOn, unfocus } = usePlanViewerContext();

  if (!building) throw new Error('You must initialize slice first!');
  const level = building.properties.levels[currentLevelIndex];

  const featureMap = new Map<ItEquipmentId, ITEquipment>(level.infrastructure.map(x => [x.id, x]));
  const assignedEquipmentIds = new Set<ItEquipmentId>(level.infrastructure.map(x => x.id));
  const roomMap = new Map<RoomId, Room>((level.buildingStructure.filter(x => isRoom(x)) as Room[]).map(x => [x.id, x]));

  const setFeature = (featureId: string, feature: ITEquipment | null) =>
    dispatch(setItInfrastructureOnLevel({ levelIndex: currentLevelIndex, featureId, feature }));

  function onLayerClicked(e: L.LeafletMouseEvent) {
    L.DomEvent.stopPropagation(e);
    const layer = e.target as Layer;
    if (!(isMarkerLayer(layer) && layerHasFeatureId(layer))) return;
    if (isAnyGeomanEditModeEnabled(map.pm)) return;

    const feature = featureMap.get(layer.featureId as ItEquipmentId);
    if (!feature || !isITEquipment(feature)) return;

    focusOn(feature);
  }

  function handleCreate({ shape, layer }: { shape: PM.SUPPORTED_SHAPES; layer: Layer }) {
    if (currentStep !== CreateNewPlanStep.InfrastructureSetup || !isMarkerLayer(layer) || shape !== 'Marker') {
      console.warn('Invalid marker creation:', { shape, isMarker: isMarkerLayer(layer) });
      layer.remove();
      return;
    }

    if (!pendingEquipment) {
      console.warn('No pending equipment selected');
      layer.remove();
      return;
    }

    const geometry = roundCoordinates(getGeoJsonFeatureGeometryFrom(layer), GEOJSON_PRECISION);
    const featureId = pendingEquipment.equipment.id as string;
    const feature: ITEquipment = {
      type: 'Feature',
      id: featureId,
      geometry: geometry as ITEquipmentGeometry,
      properties: {
        meaning: 'IT-Infrastructure',
        name: pendingEquipment.equipment.name,
        linkedToAudienceId: pendingEquipment.roomId,
        cardUrl: pendingEquipment.equipment.itEquipmentCardUrl,
        historyUrl: pendingEquipment.equipment.itEquipmentHistoryUrl,
      },
    };

    const linkedRoom = roomMap.get(feature.properties.linkedToAudienceId);
    if (!linkedRoom || !isITEquipmentInsideRoom(feature, linkedRoom)) {
      layer.remove();
      return;
    }

    const workingLayer = mutateToLayerWithFeatureIdBasedOn(layer, featureId);
    workingLayer.on('click', onLayerClicked);

    setFeature(featureId, feature);
    layer.remove();
    setPendingEquipment(null);
    map.pm.disableDraw();
    setMarkerDrawModeEnabled(false);
  }

  function handleDragStart(e: L.LeafletEvent) {
    const layer = e.target as LayerWithFeatureId;
    if (!layerHasFeatureId(layer)) return;
    const feature = featureMap.get(layer.featureId as ItEquipmentId);
    if (feature && isITEquipment(feature)) {
      const roomId = feature.properties.linkedToAudienceId;
      highlightRoom(roomId);
    }
  }

  function handleDragEnd(e: L.LeafletEvent) {
    const layer = e.target as LayerWithFeatureId;
    if (!layerHasFeatureId(layer) || !isMarkerLayer(layer)) return;
    const feature = featureMap.get(layer.featureId as ItEquipmentId);
    if (feature && isITEquipment(feature)) {
      const updatedFeature: ITEquipment = {...feature, geometry: layer.toGeoJSON(GEOJSON_PRECISION).geometry};
      setFeature(feature.id, updatedFeature);
    }
  }

  function handleRemove({ layer }: { layer: L.Layer; shape: PM.SUPPORTED_SHAPES }) {
    if (currentStep !== CreateNewPlanStep.InfrastructureSetup || !isMarkerLayer(layer) || !layerHasFeatureId(layer)) return;

    setFeature(layer.featureId as ItEquipmentId, null);
    unfocus();
  }

  // Synchronize markerDrawModeEnabled with map's drawing mode
  useEffect(() => {
    const handler: PM.GlobalDrawModeToggledEventHandler = ({ shape, enabled }) => {
      if (shape === 'Marker') {
        setMarkerDrawModeEnabled(enabled);
      }
    };
    map.on('pm:globaldrawmodetoggled', handler);
    return () => {
      map.off('pm:globaldrawmodetoggled', handler);
    };
  }, [map]);

  // Open dialog and reset pendingEquipment when marker draw mode is enabled
  useEffect(() => {
    if (markerDrawModeEnabled) {
      setDialogOpen(true);
      setPendingEquipment(null);
    }
  }, [markerDrawModeEnabled]);

  // Manage drawing mode when dialog closes
  useEffect(() => {
    if (!dialogOpen) {
      if (pendingEquipment) {
        map.pm.enableDraw('Marker', {
          markerStyle: { icon: MapStyling.itInfrastructureIcon, opacity: 1 },
          ...whenEnabledOptions,
        });
      } else if (markerDrawModeEnabled) {
        map.pm.disableDraw();
        setMarkerDrawModeEnabled(false);
      }
    }
  }, [dialogOpen, pendingEquipment, map]);

  // Reset pendingEquipment if drawing mode is disabled externally
  useEffect(() => {
    if (!markerDrawModeEnabled && pendingEquipment) {
      setPendingEquipment(null);
    }
  }, [markerDrawModeEnabled]);

  // Handle marker creation on map click
  useEffect(() => {
    map.on('pm:create', handleCreate);
    return () => {
      map.off('pm:create', handleCreate);
    };
  }, [map, pendingEquipment, level]);

  const handleEquipmentSelect = (equipment: CatalogueItEquipment, roomId: number) => {
    setPendingEquipment({ equipment, roomId });
    setDialogOpen(false);
  };

  const handleDialogClose = () => {
    setDialogOpen(false);
  };

  // Highlight room when pendingEquipment is set
  useEffect(() => {
    if (pendingEquipment) {
      highlightRoom(pendingEquipment.roomId);
    }
    return () => {
      if (pendingEquipment) {
        unhighlightRoom(pendingEquipment.roomId);
      }
    };
  }, [pendingEquipment, highlightRoom, unhighlightRoom]);

  // Attach click event listeners to layers
  useEffect(() => {
    const layers = itInfrastructureLayerGroup.getLayers()
      .filter(x => layerHasFeatureId(x)) as LayerWithFeatureId[];

    layers.forEach(x => {
      x.on("click", onLayerClicked);
    });

    return () => {
      layers.forEach(x => x.off("click", onLayerClicked));
    };
  }, [level, onLayerClicked]);

  // Attach other event listeners to layers
  useEffect(() => {
    if (currentStep != CreateNewPlanStep.InfrastructureSetup) return;
    const layers = itInfrastructureLayerGroup.getLayers();
    layers.forEach(layer => {
      layer.on("pm:remove", handleRemove);
      layer.on('pm:dragstart', handleDragStart);
      layer.on('pm:dragend', handleDragEnd);
    });

    return () => {
      layers.forEach(layer => {
        layer.off("pm:remove", handleRemove);
        layer.off('pm:dragstart', handleDragStart);
        layer.off('pm:dragend', handleDragEnd);
      });
    };
  }, [level, currentStep]);

  return (
    <>
      <EnableOrDisableLayers
        enabled={currentStep === CreateNewPlanStep.InfrastructureSetup}
        layers={itInfrastructureLayerGroup.getLayers() as LayerWithFeatureId[]}
        whenDisabledOptions={whenDisabledOptions}
        whenEnabledOptions={whenEnabledOptions}
      />
      <ItEquipmentDialog
        assignedEquipment={assignedEquipmentIds}
        roomIds={buildingInfoFromCatalogue.levels[currentLevelIndex]?.rooms.map(x => x.roomId) ?? []}
        onSelected={handleEquipmentSelect}
        open={dialogOpen}
        onClose={handleDialogClose}
      />
    </>
  );
}