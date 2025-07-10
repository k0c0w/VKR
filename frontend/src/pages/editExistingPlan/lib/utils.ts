import { GEOJSON_PRECISION } from "@app/config/constants";
import { roundCoordinates } from "@shared/map";
import { LineString, Point, Polygon } from "geojson";
import * as turf from "@turf/turf";
import { mapRoomType, UpdateInstructions } from "@features/plans";
import { Building, isRoom, isWall, Level, Room, Wall, ITEquipment } from "@entities/map";

function areGeometriesEqual(geo1: any, geo2: any): boolean {
  if (!geo1 || !geo2 || geo1.type !== geo2.type) return false;
  try {
    const roundedGeo1 = roundCoordinates(geo1, GEOJSON_PRECISION);
    const roundedGeo2 = roundCoordinates(geo2, GEOJSON_PRECISION);
    return turf.booleanEqual(roundedGeo1, roundedGeo2);
  } catch {
    return false;
  }
}

function getLevelNumber(level: Level, levelIndex: number): number {
  return level.number || parseInt(level.name) || (levelIndex + 1);
}

function createStructureMap(structure: Array<Room | Wall>): Map<number | string, Room | Wall> {
  return new Map<number | string, Room | Wall>(structure.map(item => [item.id, item]));
}

function generateBuildingUpdate(before: Building, after: Building): UpdateInstructions {
  const updates: UpdateInstructions = [];
  if (before.properties.name !== after.properties.name || !areGeometriesEqual(before.geometry, after.geometry)) {
    updates.push({
      updateType: 1, // BuildingUpdate
      updateActionType: 0, // Update
      newGeometry: !areGeometriesEqual(before.geometry, after.geometry) ? (after.geometry as Polygon) : undefined,
    });
  }
  return updates;
}

function generateRoomUpdates(
  beforeMap: Map<number | string, Room | Wall>,
  afterMap: Map<number | string, Room | Wall>,
  levelNumber: number
): UpdateInstructions {
  const updates: UpdateInstructions = [];

  // Detect deletions and updates
  beforeMap.forEach((beforeItem, id) => {
    if (!isRoom(beforeItem)) return;
    const roomBefore = beforeItem as Room;
    const afterItem = afterMap.get(id);

    if (!afterItem) {
      updates.push({
        updateType: 2, // RoomUpdate
        updateActionType: 1, // Delete
        levelNumber,
        id: roomBefore.id as number,
      });
    } else if (isRoom(afterItem)) {
      const roomAfter = afterItem as Room;
      if (
        !areGeometriesEqual(roomBefore.geometry, roomAfter.geometry) ||
        roomBefore.properties.type !== roomAfter.properties.type ||
        roomBefore.properties.name !== roomAfter.properties.name
      ) {
        updates.push({
          updateType: 2, // RoomUpdate
          updateActionType: 0, // Update
          levelNumber,
          id: roomAfter.id as number,
          newGeometry: !areGeometriesEqual(roomBefore.geometry, roomAfter.geometry)
            ? (roomAfter.geometry as Polygon)
            : undefined,
          newRoomType: roomBefore.properties.type !== roomAfter.properties.type
            ? mapRoomType(roomAfter.properties.type)
            : undefined,
          newName: roomBefore.properties.name !== roomAfter.properties.name
            ? roomAfter.properties.name
            : undefined,
        });
      }
    }
  });

  // Detect new rooms
  afterMap.forEach((afterItem, id) => {
    if (!isRoom(afterItem) || beforeMap.has(id)) return;
    const room = afterItem as Room;
    updates.push({
      updateType: 2, // RoomUpdate
      updateActionType: 2, // Create
      levelNumber,
      id: room.id as number,
      newGeometry: room.geometry as Polygon,
      newRoomType: mapRoomType(room.properties.type),
      newName: room.properties.name,
    });
  });

  return updates;
}

function generateWallUpdates(
  beforeMap: Map<number | string, Room | Wall>,
  afterMap: Map<number | string, Room | Wall>,
  levelNumber: number
): UpdateInstructions {
  const updates: UpdateInstructions = [];

  // Detect deletions and updates
  beforeMap.forEach((beforeItem, id) => {
    if (!isWall(beforeItem)) return;
    const wallBefore = beforeItem as Wall;
    const afterItem = afterMap.get(id);

    if (!afterItem) {
      updates.push({
        updateType: 3, // WallUpdate
        updateActionType: 1, // Delete
        levelNumber,
        id: wallBefore.id as string,
      });
    } else if (isWall(afterItem)) {
      const wallAfter = afterItem as Wall;
      if (!areGeometriesEqual(wallBefore.geometry, wallAfter.geometry)) {
        updates.push({
          updateType: 3, // WallUpdate
          updateActionType: 0, // Update
          levelNumber,
          id: wallAfter.id as string,
          newGeometry: wallAfter.geometry as LineString,
        });
      }
    }
  });

  // Detect new walls
  afterMap.forEach((afterItem, id) => {
    if (!isWall(afterItem) || beforeMap.has(id)) return;
    const wall = afterItem as Wall;
    updates.push({
      id: wall.id,
      updateType: 3, // WallUpdate
      updateActionType: 2, // Create
      levelNumber,
      newGeometry: wall.geometry as LineString,
    });
  });

  return updates;
}

function generateItEquipmentUpdates(beforeLevel: Level, afterLevel: Level, levelNumber: number): UpdateInstructions {
  const updates: UpdateInstructions = [];

  const beforeInfraMap = new Map<string, ITEquipment>(
    beforeLevel.infrastructure.map(item => [item.id, item])
  );
  const afterInfraMap = new Map<string, ITEquipment>(
    afterLevel.infrastructure.map(item => [item.id, item])
  );

  // Detect updates (no Create or Delete)
  beforeInfraMap.forEach((beforeItem, id) => {
    const afterItem = afterInfraMap.get(id);
    if (afterItem && !areGeometriesEqual(beforeItem.geometry, afterItem.geometry)) {
      updates.push({
        updateType: 4, // ItEquipmentUpdate
        updateActionType: 0, // Update
        levelNumber,
        id: afterItem.id,
        newGeometry: afterItem.geometry as Point,
      });
    }
  });

  return updates;
}

function generateLevelUpdates(beforeLevel: Level, afterLevel: Level, levelIndex: number): UpdateInstructions {
  const levelNumber = afterLevel.number;
  const updates: UpdateInstructions = [];

  const beforeStructureMap = createStructureMap(beforeLevel.buildingStructure);
  const afterStructureMap = createStructureMap(afterLevel.buildingStructure);

  updates.push(...generateRoomUpdates(beforeStructureMap, afterStructureMap, levelNumber));
  updates.push(...generateWallUpdates(beforeStructureMap, afterStructureMap, levelNumber));
  updates.push(...generateItEquipmentUpdates(beforeLevel, afterLevel, levelNumber));

  return updates;
}

export function generateUpdateInstructions(before: Building, after: Building): UpdateInstructions {
  const updates: UpdateInstructions = [];

  // Generate building updates
  updates.push(...generateBuildingUpdate(before, after));

  // Compare levels
  const beforeLevels = before.properties.levels;
  const afterLevels = after.properties.levels;

  if (beforeLevels.length !== afterLevels.length) {
    console.warn("Level count mismatch; updates may be incomplete.");
  }

  // Process each level
  const maxLevels = Math.max(beforeLevels.length, afterLevels.length);
  for (let levelIndex = 0; levelIndex < maxLevels; levelIndex++) {
    const beforeLevel = beforeLevels[levelIndex];
    const afterLevel = afterLevels[levelIndex];

    if (!beforeLevel || !afterLevel) {
      console.error(`Level at index ${levelIndex} missing`);
      continue;
    }

    updates.push(...generateLevelUpdates(beforeLevel, afterLevel, levelIndex));
  }

  return updates;
}