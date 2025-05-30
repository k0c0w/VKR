import { guid } from "@shared/types/guid";

export interface IGetPlanListResult {
    buildingId: guid;
    buildingName: string;
    buildingAddress: string;
}
