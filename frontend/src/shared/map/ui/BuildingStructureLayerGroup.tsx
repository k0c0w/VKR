import { useEffect } from "react";
import { useBuildingMap } from "./BuildingMapContext";
import { isRoom, isWall, Room, Wall, roomHasCompleteState } from "@entities/map";
import { mutateToLayerWithFeatureIdBasedOn } from "@shared/map/lib/leafletTypeExtensions";
import { LatLng, GeoJSON } from "leaflet";
import { BuildingMapPanes } from "./BuildingMapPanes";
import { wallStyle, getStyleByRoomType, hightLightStokeStyle, errorStyle } from "../lib/styling/styling";
import { isValidFeaturePosition } from "../lib/layerValidation";



export default function BuildingStructureLayerGroup() {
  const { map, buildingStructureLayerGroup, building, currentLevelIndex, highlightedRoomIds } = useBuildingMap();
  const features = building.properties.levels[currentLevelIndex].buildingStructure;
  
  useEffect(() => {
    buildingStructureLayerGroup.clearLayers();

    const paneOptions = { pane: BuildingMapPanes.buildingStructure.pane };

    const walls = features.filter(x => isWall(x)) as Wall[];
    const rooms = features.filter(x => isRoom(x)) as Room[];

    rooms.forEach((feature: Room) => {
      const latlngs = feature.geometry.coordinates.map((positions) =>
        GeoJSON.coordsToLatLngs(positions)
      ) as LatLng[][];
      let style;
      if (highlightedRoomIds.hasOwnProperty(feature.id)) {
        style = { ...getStyleByRoomType(feature.properties.type), ...hightLightStokeStyle };
      } else if (!roomHasCompleteState(feature) || !isValidFeaturePosition({feature, bounds: building.geometry, walls, rooms})) {
        style = errorStyle;
      } else {
        style = getStyleByRoomType(feature.properties.type);
      }
      const polygon = L.polygon(latlngs, { ...paneOptions, ...style });
      const polygonWithId = mutateToLayerWithFeatureIdBasedOn(polygon, feature.id);
      polygonWithId.addTo(buildingStructureLayerGroup);
    });

    walls.forEach((feature: Wall) => {
      const coords = feature.geometry.coordinates.map((position) =>
        GeoJSON.coordsToLatLng(position as [number, number])
      ) as LatLng[];

      const layer = L.polyline(coords, paneOptions);
      if (!isValidFeaturePosition({feature, bounds: building.geometry, walls, rooms})) {
        layer.setStyle(errorStyle);
      } else {
        layer.setStyle(wallStyle);
      }
      const layerWithId = mutateToLayerWithFeatureIdBasedOn(layer, feature.id);
      layerWithId.addTo(buildingStructureLayerGroup);
    });
  }, [map, buildingStructureLayerGroup, features, building, highlightedRoomIds]);

  return <></>;
}