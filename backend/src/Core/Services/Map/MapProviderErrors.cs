using Domain.Errors;

namespace Services.Map;

public static class MapProviderErrors
{
    public static ErrorMessage GlobalError = ErrorMessage.DomainError("Не удалось получить информацию об адресе.");
    
    public static ErrorMessage BuildingNotFoundError = ErrorMessage.EntityNotfoundError;

    public static ErrorMessage RateLimitError = ErrorMessage.DomainError("Слишком много обращений.");
}