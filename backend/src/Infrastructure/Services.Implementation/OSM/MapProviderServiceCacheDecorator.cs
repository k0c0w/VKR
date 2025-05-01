using Services.Map;

namespace Services.Implementation.OSM;

public class MapProviderServiceCacheDecorator : IMapProviderService
{
    private readonly IMapProviderService _original;

    public MapProviderServiceCacheDecorator(IMapProviderService originalService)
    {
        _original = originalService;
    }

    public Task<Dictionary<string, object>> GetBuildingInformationAsync(string city, string street, string houseNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}