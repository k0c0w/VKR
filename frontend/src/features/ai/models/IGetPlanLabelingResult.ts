import guid from "@shared/types/guid";

export interface IGetPlanLabelingResultArgs {
  requestId: guid;
}

export interface RoomOnImage {
  text?: string;
  mask: string;
  bbox: number[];
}

enum Status {
    pending = 1,
    completed = 2,
}

export interface IGetPlanLabelingResultResponse {
  requestId: guid;
  status: Status;
  error?: string;
  result?: RoomOnImage[];
}

type CompletedResponse = IGetPlanLabelingResultResponse & {status: Status.completed};

type ResultResponse = CompletedResponse & {error: undefined; result: RoomOnImage[]}

type ErrorResponse = CompletedResponse & {error: string; result: undefined};

function isIGetPlanLabelingResultResponse(response: object): response is IGetPlanLabelingResultResponse {
  return (
    'requestId' in response &&
    'status' in response &&
    typeof (response.status) === 'number' &&
    [1, 2].includes(response.status)
  );
}
function isCompletedResponse(response: IGetPlanLabelingResultResponse): response is CompletedResponse {
    return response.status == Status.completed;
}

export function isPendingResponse(response: object): response is IGetPlanLabelingResultResponse & {status: Status.pending} {
    return isIGetPlanLabelingResultResponse(response) && response.status == Status.pending;
}

export function isErrorResponse(response: object): response is ErrorResponse {
    return isIGetPlanLabelingResultResponse(response)
    && isCompletedResponse(response) 
    && response.error !== undefined
    && typeof response.error === 'string';
}

export function isResultResponse(response: object): response is ResultResponse {
    return isIGetPlanLabelingResultResponse(response)
    && isCompletedResponse(response)
    && response.result !== undefined
    && Array.isArray(response.result);
}
