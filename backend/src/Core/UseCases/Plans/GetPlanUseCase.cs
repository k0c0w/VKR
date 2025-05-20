using Domain.Errors;
using Domain.Repositories;
using ResultMonad;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class GetPlanUseCase(
    IBuildingRepository buildingRepository)
    : IUseCase<GetPlanUseCase.GetPlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    public sealed record GetPlanUseCaseArgs(Guid BuildingId);
    
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(GetPlanUseCaseArgs args, CancellationToken ct)
    { 
        var searchFilter = IBuildingRepository.BuildingFilter.IdFilter(args.BuildingId);
        
        var buildingGetResult = await buildingRepository.GetBuildingAsync(searchFilter, ct);

        if (buildingGetResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(buildingGetResult.Error);
        }

        var planRepresentingBuilding = new BuildingPlan(buildingGetResult.Value);

        return Result.Ok<BuildingPlan, ErrorMessage>(planRepresentingBuilding);
    }
}