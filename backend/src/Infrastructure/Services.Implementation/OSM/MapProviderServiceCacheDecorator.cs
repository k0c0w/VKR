using Domain.Errors;
using Domain.ValueObjects;
using Newtonsoft.Json;
using ResultMonad;
using Services.Map;
using ZiggyCreatures.Caching.Fusion;

namespace Services.Implementation.OSM;

public class MapProviderServiceCacheDecorator : IMapProviderService
{
    private const string CachePrefix = "overpass_api";
    private static readonly string NotFoundError = MapProviderErrors.BuildingNotFoundError.ToString();
    private readonly IMapProviderService _original;
    private readonly IFusionCache _cache;

    public MapProviderServiceCacheDecorator(IMapProviderService originalService, IFusionCache cache)
    {
        _original = originalService;
        _cache = cache;
    }

    public async Task<Result<BuildingBasementInformation, ErrorMessage>> GetBuildingInformationAsync(Address address,
        CancellationToken ct)
    {
        var cacheKey = $"{CachePrefix}:{address}";
        var cachedResult = await TryFindInCacheAsync(cacheKey, ct);
        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        var buildingInformationResult = await _original.GetBuildingInformationAsync(address, ct);

        if (buildingInformationResult.IsSuccess)
        {
            await CacheForDayAsync(cacheKey, buildingInformationResult.Value!, ct);
        }
        else if (buildingInformationResult.Error == MapProviderErrors.BuildingNotFoundError)
        {
            await CacheForDayAsync(cacheKey, NotFoundError, ct);
        }

        return buildingInformationResult;
    }

    private async ValueTask<Result<BuildingBasementInformation, ErrorMessage>?> TryFindInCacheAsync(string key,
        CancellationToken ct)
    {
        var cachedBuildingInformationSerialized = await _cache.GetOrDefaultAsync<string?>(key, token: ct);
        if (cachedBuildingInformationSerialized is null)
        {
            return null;
        }

        if (cachedBuildingInformationSerialized == NotFoundError)
        {
            return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
        }

        try
        {
            var cachedBuildingInformation =
                JsonConvert.DeserializeObject<BuildingBasementInformation>(cachedBuildingInformationSerialized);
            if (cachedBuildingInformation is not null)
            {
                return Result.Ok<BuildingBasementInformation, ErrorMessage>(cachedBuildingInformation);
            }
        }
        catch (JsonException)
        {
            await _cache.RemoveAsync(key, token: ct);
        }

        return null;
    }

    private ValueTask CacheForDayAsync<TValue>(string key, TValue value, CancellationToken ct)
        => _cache.SetAsync(key, value, options => { options.Duration = TimeSpan.FromDays(1); }, token: ct);
}