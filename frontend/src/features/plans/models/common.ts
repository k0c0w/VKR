import { RoomType } from "@entities/map";

export type ServerRoomType = number;
export function mapRoomType(rt:RoomType):ServerRoomType  {
    switch(rt) {
        case RoomType.Audience:
            return 1;
        case RoomType.Hall:
            return 2;
        default:
            console.warn("Fall out of RoomType Enum");
            return 1;
    }
}

export function fromServerRoomType(rt: ServerRoomType): RoomType {
    switch(rt) {
        case 1:
            return RoomType.Audience;
        case 2:
            return RoomType.Hall;
        default:
            console.warn("Fall out of RoomType Enum");
            return RoomType.Audience;
    }
}