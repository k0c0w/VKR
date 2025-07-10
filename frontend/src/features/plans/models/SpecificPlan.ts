import guid from "@shared/types/guid";
import { LineString, Point, Polygon } from "geojson";
import { fromServerRoomType, ServerRoomType } from "./common";
import { Building as DomianBuilding, Level as DomainLevel, Room as DomainRoom, Wall as DomainWall } from "@entities/map";
import { ITEquipment as ITInfrastructure } from "@entities/map/models/ItEquipment";

export interface ISpecificPlan {
    id: guid;
    buildingName: string;
    address: string; 
    region: string;
    basementGeometry: Polygon;
    levels: Level[];
};

type Structure = ({geometry: Polygon; meaning: "Room"; name: string; id:number; type: ServerRoomType;}
        |{geometry:LineString; meaning:"Wall", id:guid});

type ItEquipment = {
    relatedToRoomId: number;
    inventoryNumber: string;
    locationPoint: Point;
    name: string;
    cardUrl: string;
    historyUrl: string;
}

type Level = {
    name: string;
    number: number;
    structure: Structure[];
    itEquipments: ItEquipment[];
}

export interface ISpecificPlanArgs {
    id: guid;
}

export function planResponseToBuilding(plan: ISpecificPlan): DomianBuilding {
    const data = plan;
    const building: DomianBuilding = {
        id: data.id,
        geometry: data.basementGeometry,
        type: "Feature",
        properties: {
            name: data.buildingName,
            address: data.address,
            region: data.region,
            levels: data.levels.map((l, i) => ({
                name: l.name,
                number: l.number,
                infrastructure: l.itEquipments?.map(ie => ({
                    id: ie.inventoryNumber,
                    type: "Feature",
                    geometry: ie.locationPoint,
                    properties: {
                        cardUrl: ie.cardUrl,
                        historyUrl: ie.historyUrl,
                        linkedToAudienceId: ie.relatedToRoomId,
                        name: ie.name,
                        meaning: "IT-Infrastructure"
                    }
                } as ITInfrastructure)) ?? [],
                buildingStructure: l.structure?.map(s => {
                    if (s.meaning === "Wall") {
                        const wall: DomainWall = {
                            id: s.id,
                            type: "Feature",
                            geometry: s.geometry,
                            properties:{meaning: "Wall"}
                        }
                        return wall;
                    }
                    if (s.meaning === "Room") {
                        const room: DomainRoom = {
                            id: s.id,
                            type: "Feature",
                            geometry: s.geometry,
                            properties: {
                                meaning: "Room",
                                type: fromServerRoomType(s.type),
                                name: s.name,
                            }
                        }
                        return room;
                    }

                    throw new Error("Unknown structure type.");
                })
            })) as DomainLevel[] 
        }
    }as DomianBuilding;

    return building;
} 