import guid from "@shared/types/guid";

export interface IRequestPlanLabelingArgs {
    planImage: Blob;
}

export interface IRequestPlanLabelingResponse {
    requestId: guid;
}