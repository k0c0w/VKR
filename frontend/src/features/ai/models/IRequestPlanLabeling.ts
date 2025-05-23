import { guid } from "@shared/types/guid";

export interface IRequestPlanLabelingArgs {
    planImage: string;
}

export interface IRequestPlanLabelingResponse {
    requiestId: guid;
    estimatedTime: number;
}