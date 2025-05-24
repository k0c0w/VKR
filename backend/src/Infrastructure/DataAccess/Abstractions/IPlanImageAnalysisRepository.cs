using DataAccess.Models;

namespace DataAccess.Abstractions;

public interface IPlanImageAnalysisRepository
{
    public Task AddAsync(PlanImageAnalysisResult value, CancellationToken ct);

    public Task<PlanImageAnalysisResult?> GetAsync(string key, CancellationToken ct);
}