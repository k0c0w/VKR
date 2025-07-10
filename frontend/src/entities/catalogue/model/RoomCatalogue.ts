import { RoomId } from "@shared/types/valueObjects"

export type RoomsCatalogue = {
    [levelName: string]: RoomId[];
}