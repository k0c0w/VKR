import { Polygon, LineString } from "geojson";

export interface IInfrastructure {

}

export enum RoomType {
    Audience = 0,
    Hall = 1,
}

export type Wall = {
    boundaries: LineString;
}

export type Room = {
    id: number;
    type: RoomType;
    name?: string;
    boundaries: Polygon;
    infrastructure: IInfrastructure[];
}