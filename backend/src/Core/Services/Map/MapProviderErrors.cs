using Domain.Errors;

namespace Services.Map;

public static class MapProviderErrors
{
    public static ErrorMessage GlobalError = new ("Не удалось получить информацию об адресе.");
    
    public static ErrorMessage BuildingNotFoundError = ErrorMessage.EntityNotfoundError;
}