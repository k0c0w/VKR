import { Building, isRoom, Room } from "@entities/map";
import PlanViewerBaseWidget from "./PlanViewerBaseWidget";
import ItEquipmentPopup from "./ItEquipmentPopup";
import RoomDescriptionPopup from "./RoomPopup";
import { usePlanViewerContext } from "./PlanViewerContext";
import { useBuildingMap } from "@shared/map";
import { useCallback, useEffect } from "react";
import { layerHasFeatureId } from "@shared/map/lib/leafletTypeExtensions";
import { ITEquipment } from "@entities/map/models/ItEquipment";


export default function PlanViewerWidget({building}: {building: Building;}) {

    return (
        <PlanViewerBaseWidget building={building} readonlyMode>
            <RoomDescriptionPopup />
            <ItEquipmentPopup />
            <SubscribeOnClickEvents />
        </PlanViewerBaseWidget>
    );
}

function SubscribeOnClickEvents() {
    const { focusOn } = usePlanViewerContext();
    const { buildingStructureLayerGroup, itInfrastructureLayerGroup, building, currentLevelIndex } = useBuildingMap();
    const level = building.properties.levels[currentLevelIndex];
    const levelStructureFeatures = level.buildingStructure.filter(isRoom) as Room[];
    const itEquipmentFeatures = level.infrastructure as ITEquipment[];

    const onRoomLayerClick = useCallback((e: L.LeafletMouseEvent) => {
        L.DomEvent.stopPropagation(e);
        const layer = e.target;
        if (!layerHasFeatureId(layer)) {
            return;
        }

        const featureId = layer.featureId;
        const feature = levelStructureFeatures.find(x => x.id === featureId);

        if (feature) {
            focusOn(feature);
        }
    }, [levelStructureFeatures, focusOn]);

    const onItEquipmentClick = useCallback((e: L.LeafletMouseEvent) => {
        L.DomEvent.stopPropagation(e);
        const layer = e.target;
        if (!layerHasFeatureId(layer)) {
            return;
        }

        const featureId = layer.featureId;
        const feature = itEquipmentFeatures.find(x => x.id === featureId);

        if (feature) {
            focusOn(feature);
        }
    }, [itEquipmentFeatures, focusOn]);

    useEffect(() => {
        const structureLayers = buildingStructureLayerGroup.getLayers();
        structureLayers.forEach(layer => {
            layer.on("click", onRoomLayerClick);
        });

        return () => {
            structureLayers.forEach(layer => {
                layer.off("click", onRoomLayerClick);
            });
        };
    }, [buildingStructureLayerGroup, onRoomLayerClick]);

    useEffect(() => {
        const itLayers = itInfrastructureLayerGroup.getLayers();
        itLayers.forEach(layer => {
            layer.on("click", onItEquipmentClick);
        });

        return () => {
            itLayers.forEach(layer => {
                layer.off("click", onItEquipmentClick);
            });
        };
    }, [itInfrastructureLayerGroup, onItEquipmentClick]);

    return <></>;
}
