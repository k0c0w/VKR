using Domain;
using Domain.Errors;

namespace Services.Map;

public sealed record BuildingNotFoundError : ErrorMessage
{
    internal BuildingNotFoundError():base("Здание не найдено.")
    {
    }
}