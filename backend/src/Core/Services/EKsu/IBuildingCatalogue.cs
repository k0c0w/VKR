using Domain.Errors;
using ResultMonad;
using Services.EKsu.BuildingCatalogue;

namespace Services.EKsu;

public interface IBuildingCatalogue
{
    public Task<Result<Building[], ErrorMessage>> GetBuildingsAsync(string region, CancellationToken ct);

    public Task<Result<string[], ErrorMessage>> GetRegionsAsync(CancellationToken ct);

    public Task<Result<Building, ErrorMessage>> GetBuildingAsync(string region, string name, string address,
        CancellationToken ct); 
}