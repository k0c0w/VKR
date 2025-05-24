using DataAccess.Abstractions;
using DataAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

namespace DataAccess.Repositories;

public class PlanImageAnalysisRepository(IFusionCache cache) : IPlanImageAnalysisRepository
{
    public Task AddAsync(PlanImageAnalysisResult value, CancellationToken ct)
    {
        return cache.SetAsync(GetKey(value.RequestId), value, opt =>
        {
            opt.Duration = TimeSpan.FromMinutes(5);
            opt.Priority = CacheItemPriority.High;
        }, ct)
        .AsTask();
    }

    public Task<PlanImageAnalysisResult?> GetAsync(string key, CancellationToken ct)
    {
        return cache.GetOrDefaultAsync<PlanImageAnalysisResult?>(GetKey(key), token: ct)
            .AsTask();
    }

    private static string GetKey(string key) => $"plan_analysis:{key}";
}