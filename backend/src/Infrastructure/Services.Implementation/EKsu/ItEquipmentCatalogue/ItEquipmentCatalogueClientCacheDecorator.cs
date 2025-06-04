using System.Web;
using Domain.Entities;
using Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ResultMonad;
using Services.EKsu;
using Services.EKsu.Authorization;
using ZiggyCreatures.Caching.Fusion;

namespace Services.Implementation.EKsu.ItEquipmentCatalogue;

public class ItEquipmentCatalogueClientCacheDecorator(
    IItEquipmentCatalogue catalogue,
    IFusionCache cache,
    IHttpContextAccessor contextAccessor,
    ILogger<ItEquipmentCatalogueClientCacheDecorator>? logger = default)
    : IItEquipmentCatalogue
{
    private const string CachePrefix = "it_equipment_catalogue:";
    public async Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsAsync(
        long[] roomIds, CancellationToken ct = default)
    {
        var (p1, p2, ph) = GetSessionClaims();
        var itEquipment = new List<ItEquipmentDescription>();
        var toFetchIds = new List<long>(roomIds.Length);
        
        foreach(var roomId in roomIds)
        {
            var key = $"{CachePrefix}{roomId}";
            var cached = await cache.GetOrDefaultAsync<ItEquipmentDescription[]>(key, token: ct);
            if (cached is null)
            {
                logger?.LogInformation("Equipment for {RoomId} was not found in cache, adding to fetch queue.", roomId);
                toFetchIds.Add(roomId);
            }
            else
            {
                itEquipment.AddRange(cached);
            }
        }

        var fetchResult = await catalogue.GetAllItEquipmentByRoomIdsAsync(toFetchIds.ToArray(), ct);
        if (fetchResult.IsFailure)
        {
            return fetchResult;
        }

        var fetched = fetchResult.Value!;
        foreach (var byRoomGrouping in fetched
                     .Select(x => MutateUrls(x, p1, p2, ph))
                     .GroupBy(x => x.LocationAudienceCatalogueId))
        {
            var equipment = byRoomGrouping.ToArray();
            var cacheTask = cache.SetAsync($"{CachePrefix}{byRoomGrouping.Key}", equipment, options =>
            {
                options.EagerRefreshThreshold = 0.8f;
                options.Duration = TimeSpan.FromMinutes(15);
            }, token: ct);
            
            itEquipment.AddRange(equipment);

            await cacheTask;
        }

        return Result.Ok<ItEquipmentDescription[], ErrorMessage>(itEquipment.ToArray());
    }

    private (string P1, string P2, string PH) GetSessionClaims()
    {
        string p1 = string.Empty, p2 = string.Empty, ph = string.Empty;
        foreach (var claim in contextAccessor?.HttpContext?.User?.Claims ?? [])
        {
            if (claim.Type == nameof(AuthorizationCredentials.Entry))
            {
                p1 = claim.Value;
            }
            else if (claim.Type == nameof(AuthorizationCredentials.Session))
            {
                p2 = claim.Value;
            }
            else if (claim.Type == nameof(AuthorizationCredentials.Hash))
            {
                ph = claim.Value;
            }
        }

        return (P1: p1, P2: p2, PH: ph);
    }
    
    private static ItEquipmentDescription MutateUrls(ItEquipmentDescription x, string p1, string p2, string ph)
    {
        x.ItEquipmentCardUrl = RefreshCachedUrl(x.ItEquipmentCardUrl, p1, p2, ph);
        x.ItEquipmentHistoryUrl = RefreshCachedUrl(x.ItEquipmentHistoryUrl, p1, p2, ph);

        return x;
    }
    
    private static string RefreshCachedUrl(string oldUrl, string p1, string p2, string pH)
    {
        if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2) || string.IsNullOrEmpty(pH))
        {
            return oldUrl;
        }
        
        var uri = new Uri(oldUrl);
        var queryParameters = HttpUtility.ParseQueryString(uri.Query);

        if (queryParameters.Get("p_h") == pH)
        {
            return oldUrl;
        }
        
        queryParameters.Set("p1", p1);
        queryParameters.Set("p2", p2);
        queryParameters.Set("p_h", pH);

        return new UriBuilder(uri)
        {
            Query = queryParameters.ToString(),
        }.ToString();
    }
}