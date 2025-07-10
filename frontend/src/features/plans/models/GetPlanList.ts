import guid from "@shared/types/guid";

export interface IGetPlanListResult {
    planId: guid;
    region: string;
    address: string;
    buildingName: string;
}
