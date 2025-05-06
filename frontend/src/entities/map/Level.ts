import { BuildingGeometry } from "./Building";
import { Wall, Room, isWall, isWallInBounds, isRoom, isRoomInBounds, hasSelfIntersection, doesRoomIntersectOtherRooms, roomHasCompleteState, doesWallIntersetOtherWalls } from "./BuildingStructure";
import { hasCompleteState, isITInfrastructureInBounds, ITInfrastructure } from "./ITInfrastructure";

export type Level = {
    number: number;
    name: string;
    buildingStructure: LevelBuildingStructure;
    infrastructure: ITInfrastructure[]
}

// todo: level consists not only from wall and rooms, it also contains infrasrtucture and stairs, doors
export type LevelBuildingStructure = (Wall | Room)[];


export function levelHasValidState({buildingStructure, infrastructure}: Level, buildingBounds: BuildingGeometry): boolean {
    
    const rooms: Room[] = [];
    const walls: Wall[] = [];

    for(const entry of buildingStructure) {
        if (isWall(entry)) {
            if (isWallInBounds(entry, buildingBounds) && !hasSelfIntersection(entry)) {
                walls.push(entry);
            } else {
                return false;
            }
        } else if (isRoom(entry)) {
            if (isRoomInBounds(entry, buildingBounds)) {
                rooms.push(entry);
            } else {
                return false;
            }
        }
    }

    for(const wall of walls){
        if (doesWallIntersetOtherWalls(wall, walls)){
            return false;
        }
    }

    const roomsMap = new Map<string, Room>();
    for(const room of rooms) {
        if (!roomHasCompleteState(room) || doesRoomIntersectOtherRooms(room, rooms)) {
            return false;
        } else {
            roomsMap.set(room.properties.id!, room);
        }
    }

    for(const inf of infrastructure) {
        if (!(isITInfrastructureInBounds(inf, buildingBounds) && hasCompleteState(inf))) {
            return false;
        }

        if (!roomsMap.has(inf.properties.linkedToAudienceId)) {
            return false
        }
    }

    return true;
}