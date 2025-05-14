using Domain.Errors;
using Domain.Repositories;
using ResultMonad;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public sealed class GetAvailablePlansListUseCase(IBuildingRepository buildingRepository)
    : IUseCase<Result<BuildingPlanShortcut[], ErrorMessage>>
{
    
    public async Task<Result<BuildingPlanShortcut[], ErrorMessage>> RunAsync(CancellationToken ct)
    {
        var buildingInfosResult = await buildingRepository.GetAllBuildingInformationAsync(ct);
        if (buildingInfosResult.IsFailure)
        {
            return Result.Fail<BuildingPlanShortcut[], ErrorMessage>(buildingInfosResult.Error);
        }

        var allAvailablePlans = buildingInfosResult.Value
            .Select(bi => new BuildingPlanShortcut(bi.Address.ToString()))
            .ToArray();

        return Result.Ok<BuildingPlanShortcut[], ErrorMessage>(allAvailablePlans);
    }
}