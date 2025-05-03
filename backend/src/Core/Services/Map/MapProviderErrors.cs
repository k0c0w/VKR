using Domain;

namespace Services.Map;

public static class MapProviderErrors
{
    public static MapProviderError GlobalError = new();

    public static BuildingNotFoundError BuildingNotFoundError = new();
}