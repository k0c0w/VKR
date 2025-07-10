using System.Security.Claims;
using Domain.Entities;
using Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ResultMonad;
using Services.EKsu;
using ZiggyCreatures.Caching.Fusion;

namespace Services.Implementation.EKsu.ItEquipmentCatalogue;

public class ItEquipmentCatalogueClientCacheDecorator(
    IItEquipmentCatalogue catalogue,
    IHttpContextAccessor contextAccessor,
    IFusionCache cache,
    ILogger<ItEquipmentCatalogueClientCacheDecorator>? logger = default)
    : IItEquipmentCatalogue
{
    private const string CachePrefix = "it_equipment_catalogue";
    public async Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsAsync(
        long[] roomIds, CancellationToken ct = default)
    {
        var itEquipment = new List<ItEquipmentDescription>();
        var toFetchIds = new List<long>(roomIds.Length);
        
        var currentUserEmail =contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        foreach(var roomId in roomIds)
        {
            var key = $"{CachePrefix}:{currentUserEmail}:{roomId}";
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
}