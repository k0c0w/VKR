import { RoomId, WallId } from "@entities/map";
import { ItEquipmentId } from "@entities/map/models/ItEquipment";
import guid from "@shared/types/guid";
import { LineString, Point, Polygon } from "geojson";
import { ServerRoomType } from "./common";

enum UpdateInstructionType {
  BuildingUpdate = 1,
  RoomUpdate = 2,
  WallUpdate = 3,
  ItEquipmentUpdate = 4,
}

enum UpdateActionEnum {
  Update = 0,
  Delete = 1,
  Create = 2,
}

export interface IUpdatePlanArg {
    id: guid;
    updateInstructions: UpdateInstructions;
}

export interface IBuildingUpdate {
    newGeometry?: Polygon;
    updateType: UpdateInstructionType.BuildingUpdate;
    updateActionType: UpdateActionEnum.Update;
}

export interface IRoomUpdate {
    id: RoomId;
    newRoomType?: ServerRoomType;
    newGeometry?: Polygon;
    updateType: UpdateInstructionType.RoomUpdate;
    updateActionType: UpdateActionEnum;
    levelNumber: number;
    newName?: string;
}

export interface IWallUpdate {
    id: WallId;
    newGeometry?: LineString; 
    updateActionType: UpdateActionEnum;
    updateType: UpdateInstructionType.WallUpdate;
    levelNumber: number;
}

export interface IItEquipmentUpdate {
    id: ItEquipmentId;
    newGeometry?: Point;
    updateActionType: UpdateActionEnum;
    updateType: UpdateInstructionType.ItEquipmentUpdate;
    levelNumber: number;
}

export type UpdateInstructions = Array<IItEquipmentUpdate | IWallUpdate | IRoomUpdate | IBuildingUpdate>;
