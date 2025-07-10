import { SKIP_CATALOGUE_VALIDATOIN } from "@app/config/env";
import { CatalogueRoom } from "@entities/catalogue";
import { Building, hasValidState, isRoom, Room, isWall, doesRoomIntersectOtherRooms, isWallInBounds, isRoomInBounds, doesWallIntersectOtherWalls } from "@entities/map";
import { isITEquipmentInBounds } from "@entities/map/models/ItEquipment";
import * as turf from "@turf/turf";

type ValidationResult = { error: string; isValid: false } | { isValid: true; error: undefined };

export function validatePlanState(
  building: Building,
  catalogue: { rooms: CatalogueRoom[]; },
  options: { skipCatalogueChecks?: boolean } = {}
): ValidationResult {
  // Initial basic state check
  if (!hasValidState(building)) {
    return {
      isValid: false,
      error: "Убедитесь, что помещения находятся внутри границ здания и не пересекаются, стены не пересекаются, а оборудование размещено в связанном с ним помещении."
    };
  }

  const allRooms = building.properties.levels
    .flatMap(x => x.buildingStructure)
    .filter(isRoom) as Room[];

  // Catalogue validation (can be skipped during development)
  if (!SKIP_CATALOGUE_VALIDATOIN) {
    // Check if all rooms from the catalogue are placed
    const catalogueRoomsSet = new Set(catalogue.rooms.map(r => r.roomId));

    const placedRoomsSet = new Set(allRooms.map(r => r.id));
    const missingRooms = Array.from(catalogueRoomsSet).filter(id => !placedRoomsSet.has(id));
    if (missingRooms.length > 0) {
      return {
        isValid: false,
        error: `Не все комнаты из каталога размещены на плане. Отсутствующие комнаты с идентификаторами: ${missingRooms.join(', ')}`
      };
    }
  }

  // Validate positions and intersections
  for (const level of building.properties.levels) {
    const rooms = level.buildingStructure.filter(isRoom);
    const walls = level.buildingStructure.filter(isWall);

    // Check if all objects are inside the basement polygon
    for (const room of rooms) {
      if (!isRoomInBounds(room, building.geometry)) {
        return { isValid: false, error: `Комната с идентификатором ${room.id} находится вне границ.` };
      }
    }
    for (const wall of walls) {
      if (!isWallInBounds(wall, building.geometry)) {
        return { isValid: false, error: `Одна из стен находится вне границ.` };
      }
    }
    for (const equipment of level.infrastructure) {
      if (!isITEquipmentInBounds(equipment, building.geometry)) {
        return { isValid: false, error: `ИТ-оборудование ${equipment.id} (${equipment.properties.name}) находится вне границ.` };
      }
      // Check if equipment is inside its linked room
      const room = rooms.find(r => r.id === equipment.properties.linkedToAudienceId);
      if (!room || !turf.booleanPointInPolygon(equipment.geometry, room.geometry)) {
        return {
          isValid: false,
          error: `ИТ-оборудование ${equipment.id} (${equipment.properties.name}) не расположено согласно каталогу.`
        };
      }
    }

    // Check for intersections
    for (const room of rooms) {
      if (doesRoomIntersectOtherRooms(room, rooms)) {
        return { isValid: false, error: `Некоторые комнаты пересекаются.` };
      }
    }
    for (const wall of walls) {
      if (doesWallIntersectOtherWalls(wall, walls)) {
        return { isValid: false, error: `Некоторые стены пересекаются.` };
      }
    }
  }

  return { isValid: true, error: undefined };
}