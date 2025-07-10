import { mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { GeoJSON } from "leaflet";
import React from "react";
import { useBuildingMap } from "./BuildingMapContext";
import { ITEquipment } from "@entities/map/models/ItEquipment";
import { isRoom, itEquipmentHasCorrectPosition, Room } from "@entities/map";
import { MapStyling } from "..";

export default function ItEquipmentLayerGroup() {
  const { map, itInfrastructureLayerGroup, building, currentLevelIndex, highlightedItEquipmentIds } = useBuildingMap();
  const level = building.properties.levels[currentLevelIndex];
  const itEquipment = level.infrastructure;
  const rooms = level.buildingStructure.filter(x => isRoom(x)) as Room[];

  React.useEffect(() => {
    itInfrastructureLayerGroup.clearLayers();

    itEquipment.forEach((feature: ITEquipment) => {
      const latlng = GeoJSON.coordsToLatLng(feature.geometry.coordinates as [number, number]);
      const icon = feature.id in highlightedItEquipmentIds
        ? MapStyling.highlightItInfrastructureIcon
        : itEquipmentHasCorrectPosition(feature, rooms)
        ? MapStyling.itInfrastructureIcon
        : MapStyling.errorItInfrastructureIcon;
      const marker = L.marker(latlng, { icon });
      const markerWithId = mutateToLayerWithFeatureIdBasedOn(marker, feature.id);

      markerWithId.addTo(itInfrastructureLayerGroup);
    });
  }, [map, itInfrastructureLayerGroup, itEquipment, highlightedItEquipmentIds]);

  return <></>;
}