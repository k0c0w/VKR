export interface IProblemDetails {
    title: string;
    status: number;
    details?: string;
}

export interface IValidationProblemDetails extends IProblemDetails {
    title: "Ошибка валидации.";
    errors: {
        [err: string]: any
    } 
}

export function isProblemDetatils(obj: any): obj is IProblemDetails {
    return obj.type !== undefined && typeof obj.type === 'string'
        && obj.title !== undefined && typeof obj.title === 'string'
        && obj.status !== undefined && typeof obj.status === 'number' && 100 <= obj.status && obj.status < 600;
}

export function isValidationProblemDetails(obj: any): obj is IValidationProblemDetails {
    return isProblemDetatils(obj) 
        && obj.status === 400
        && obj.title === "Ошибка валидации.";
}

export function isServerErrorResponse(
    response: any
): response is { status: 500; detail?: string; title: "Ошибка сервера."; } {
    return response !== undefined && isProblemDetatils(response) && response.status === 500;
}