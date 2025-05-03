using Domain;
using Domain.Errors;
using ResultMonad;
using Services.Map;
using ZiggyCreatures.Caching.Fusion;

namespace Services.Implementation.OSM;

public class MapProviderServiceCacheDecorator : IMapProviderService
{
    private readonly IMapProviderService _original;
    private readonly IFusionCache _cache;

    public MapProviderServiceCacheDecorator(IMapProviderService originalService, IFusionCache cache)
    {
        _original = originalService;
        _cache = cache;
    }

    public async Task<Result<BuildingInformation, ErrorMessage>> GetBuildingInformationAsync(Address address, CancellationToken ct)
    {
        var cacheKey = address.ToString();
        var cachedBuildingInformation = await _cache.GetOrDefaultAsync<BuildingInformation?>(cacheKey, token: ct);

        if (cachedBuildingInformation is not null)
        {
            return Result.Ok<BuildingInformation, ErrorMessage>(cachedBuildingInformation);
        }

        var buildingInformationResult = await _original.GetBuildingInformationAsync(address, ct);
        if (buildingInformationResult.IsSuccess)
        {
            await _cache.SetAsync(cacheKey, buildingInformationResult.Value!, options =>
            {
                options.Duration = TimeSpan.FromDays(1);
            }, token: ct);
        }
        
        return buildingInformationResult;
    }
}