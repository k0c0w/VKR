import { Wall, Room } from "./BuildingStructure";
import { ITInfrastructure } from "./ITInfrastructure";

export type Level = {
    number: number;
    name: string;
    buildingStructure: LevelBuildingStructure;
    infrastructure: ITInfrastructure[]
}

// todo: level consists not only from wall and rooms, it also contains infrasrtucture and stairs, doors
export type LevelBuildingStructure = (Wall | Room)[];
