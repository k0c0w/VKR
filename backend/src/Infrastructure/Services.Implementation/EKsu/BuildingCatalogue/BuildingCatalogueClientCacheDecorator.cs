using Domain.Errors;
using ResultMonad;
using Services.EKsu;
using Services.EKsu.BuildingCatalogue;
using ZiggyCreatures.Caching.Fusion;

public class BuildingCatalogueClientCacheDecorator : IBuildingCatalogue
{
    private const string CacheNameSpace = "building_catalogue";
    private readonly IBuildingCatalogue _buildingCatalogue;
    private readonly IFusionCache _cache;

    public BuildingCatalogueClientCacheDecorator(
        IBuildingCatalogue buildingCatalogue,
        IFusionCache cache)
    {
        _buildingCatalogue = buildingCatalogue;
        _cache = cache;
    }

    public async Task<Result<Building[], ErrorMessage>> GetBuildingsAsync(string region, CancellationToken ct)
    {
        var cacheKey = $"{CacheNameSpace}:{region}";
        
        var cachedResult = await _cache.GetOrDefaultAsync<Building[]>(cacheKey, token: ct);
        if (cachedResult != null)
        {
            return Result.Ok<Building[], ErrorMessage>(cachedResult);
        }

        var result = await _buildingCatalogue.GetBuildingsAsync(region, ct);
        if (result.IsSuccess)
        {
            await _cache.SetAsync(
                cacheKey,
                result.Value,
                options => options
                    .SetDuration(TimeSpan.FromMinutes(30))
                    .SetFailSafe(true, TimeSpan.FromHours(1))
                    .SetFactoryTimeouts(TimeSpan.FromSeconds(5)),
                ct);
        }

        return result;
    }

    public async Task<Result<string[], ErrorMessage>> GetRegionsAsync(CancellationToken ct)
    {
        const string cacheKey = $"{CacheNameSpace}:regions";
        
        var cachedResult = await _cache.GetOrDefaultAsync<string[]>(cacheKey, token: ct);
        if (cachedResult != null)
        {
            return Result.Ok<string[], ErrorMessage>(cachedResult);
        }

        var result = await _buildingCatalogue.GetRegionsAsync(ct);
        if (result.IsSuccess)
        {
            await _cache.SetAsync(
                cacheKey,
                result.Value,
                options => options
                    .SetDuration(TimeSpan.FromMinutes(30))
                    .SetFailSafe(true, TimeSpan.FromDays(1))
                    .SetFactoryTimeouts(TimeSpan.FromSeconds(3)),
                ct);
        }

        return result;
    }

    public async Task<Result<Building, ErrorMessage>> GetBuildingAsync(string region, string name, string address, CancellationToken ct)
    {
        var cacheKey = $"{CacheNameSpace}:{region}:{name}:{address}";
        
        var cachedResult = await _cache.GetOrDefaultAsync<Building>(cacheKey, token:ct);
        if (cachedResult != null)
        {
            return Result.Ok<Building, ErrorMessage>(cachedResult);
        }

        var result = await _buildingCatalogue.GetBuildingAsync(region, name, address, ct);
        if (result.IsSuccess)
        {
            await _cache.SetAsync(
                cacheKey,
                result.Value,
                options => options
                    .SetDuration(TimeSpan.FromMinutes(15))
                    .SetFailSafe(true, TimeSpan.FromHours(1))
                    .SetFactoryTimeouts(TimeSpan.FromSeconds(5)),
                ct);
        }

        return result;
    }
}