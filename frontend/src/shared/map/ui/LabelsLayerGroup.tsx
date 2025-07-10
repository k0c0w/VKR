import React, { useEffect, useState } from "react";
import { useBuildingMap } from "./BuildingMapContext";
import { isRoom, Room } from "@entities/map";
import { mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { LatLng, GeoJSON, LayerGroup } from "leaflet";
import { BuildingMapPanes } from "./BuildingMapPanes";
import { MapStyling } from "..";

export default function RoomLabelsLayer() {
  const { map, labelsLayerGroup, building, currentLevelIndex, highlightedRoomIds } = useBuildingMap();
  const features = building.properties.levels[currentLevelIndex].buildingStructure;
  const [currentZoom, setCurrentZoom] = useState(map.getZoom());

  useEffect(() => {
    const handleZoomEnd = () => {
      setCurrentZoom(map.getZoom());
    };

    map.on('zoomend', handleZoomEnd);

    return () => {
      map.off('zoomend', handleZoomEnd);
    };
  }, [map]);


  useEffect(() => {
    labelsLayerGroup.clearLayers();

    const paneOptions = { pane: BuildingMapPanes.labels.pane };

  (features.filter(x => isRoom(x)) as Room[])
    .forEach((feature: Room) => {
      if (feature.properties.name && currentZoom >= MapStyling.minZoomForLabels) {
        const cleanedName = feature.properties.name.replace(/\s*\([^)]*\)/g, '').trim();
        if (cleanedName) {
          // Compute polygon center directly from GeoJSON coordinates
          const latlngs = feature.geometry.coordinates.map((positions) =>
            GeoJSON.coordsToLatLngs(positions)
          ) as LatLng[][];
          const polygon = L.polygon(latlngs);
          const center = polygon.getBounds().getCenter();
          const nameIcon = MapStyling.getRoomLabel(cleanedName, currentZoom);
          const nameMarker = L.marker(center, { icon: nameIcon, interactive: false, ...paneOptions });
          const nameMarkerWithId = mutateToLayerWithFeatureIdBasedOn(nameMarker, `${feature.id}_name`);
          nameMarkerWithId.addTo(labelsLayerGroup);
        }
      }
    });
  }, [map, labelsLayerGroup, features, building, highlightedRoomIds, currentZoom]);

  return <></>;
}