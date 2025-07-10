import React, { createContext, useContext, useRef, useState } from "react";
import { Map as LeafletMap, LayerGroup, Polygon as LeafletPolygon } from "leaflet";
import { useMap } from "react-leaflet";
import { Building, RoomId } from "@entities/map";
import { ItEquipmentId } from "@entities/map/models/ItEquipment";

interface BuildingMapContextType {
  map: LeafletMap;
  basementPolygonRef: React.RefObject<LeafletPolygon | null>;
  buildingStructureLayerGroup: LayerGroup;
  itInfrastructureLayerGroup: LayerGroup;
  labelsLayerGroup: LayerGroup;
  currentLevelIndex: number;
  setCurrentLevelIndex: (index: number) => void;
  building: Building;
  highlightedRoomIds: { [roomId: RoomId]: void };
  highlightRoom: (roomId: RoomId) => void;
  unhighlightRoom: (roomId: RoomId) => void;
  clearHighlightedRooms: () => void;
  highlightedItEquipmentIds: { [itEquipmentId: ItEquipmentId]: void };
  highlightItEquipment: (itEquipmentId: ItEquipmentId) => void;
  unhighlightItEquipment: (itEquipmentId: ItEquipmentId) => void;
  clearHighlightedItEquipment: () => void;
}

const BuildingMapContext = createContext<BuildingMapContextType | undefined>(undefined);

export const useBuildingMap = () => {
  const context = useContext(BuildingMapContext);
  if (!context) {
    throw new Error("useBuildingMapContext must be used within a BuildingMapProvider");
  }
  return context;
};

interface BuildingMapProviderProps {
  building: Building;
  initialLevelIndex: number;
  children: React.ReactNode;
}

export function BuildingMapProvider({ building, initialLevelIndex, children }: BuildingMapProviderProps) {
  const map = useMap();
  const basementPolygonRef = useRef<LeafletPolygon | null>(null);
  const buildingStructureLayerGroup = useRef<LayerGroup>(L.layerGroup().addTo(map)).current;
  const itInfrastructureLayerGroup = useRef<LayerGroup>(L.layerGroup().addTo(map)).current;
  const labelsLayerGroup = useRef<LayerGroup>(L.layerGroup().addTo(map)).current;
  const [currentLevelIndex, setCurrentLevelIndex] = useState(initialLevelIndex);
  const [highlightedRoomIds, setHighlightedRoomIds] = useState<{ [roomId: RoomId]: void }>({});
  const [highlightedItEquipmentIds, setHighlightedItEquipmentIds] = useState<{ [itEquipmentId: string]: void }>({});

  const highlightRoom = (roomId: RoomId) => {
    setHighlightedRoomIds((prev) => ({ ...prev, [roomId]: void 0 }));
  };

  const unhighlightRoom = (roomId: RoomId) => {
    setHighlightedRoomIds((prev) => {
      const newObj = { ...prev };
      delete newObj[roomId];
      return newObj;
    });
  };

  const clearHighlightedRooms = () => {
    setHighlightedRoomIds({});
  };

  const highlightItEquipment = (itEquipmentId: ItEquipmentId) => {
    setHighlightedItEquipmentIds((prev) => ({ ...prev, [itEquipmentId]: void 0 }));
  };

  const unhighlightItEquipment = (itEquipmentId: ItEquipmentId) => {
    setHighlightedItEquipmentIds((prev) => {
      const newObj = { ...prev };
      delete newObj[itEquipmentId];
      return newObj;
    });
  };

  const clearHighlightedItEquipment = () => {
    setHighlightedItEquipmentIds({});
  };

  const contextValue: BuildingMapContextType = {
    map,
    basementPolygonRef,
    buildingStructureLayerGroup,
    itInfrastructureLayerGroup,
    labelsLayerGroup,
    currentLevelIndex,
    setCurrentLevelIndex,
    building,
    highlightedRoomIds,
    highlightRoom,
    unhighlightRoom,
    clearHighlightedRooms,
    highlightedItEquipmentIds,
    highlightItEquipment,
    unhighlightItEquipment,
    clearHighlightedItEquipment,
  };

  return <BuildingMapContext.Provider value={contextValue}>{children}</BuildingMapContext.Provider>;
}