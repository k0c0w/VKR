using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using ResultMonad;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class GetPlanUseCase(
    IAuthorizationService authorizationService,
    IBuildingRepository buildingRepository)
    : WithAuthorizeUseCaseBase(authorizationService), 
      IUseCase<GetPlanUseCase.GetPlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    public sealed record GetPlanUseCaseArgs(Guid BuildingId);
    
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(GetPlanUseCaseArgs args, CancellationToken ct)
    { 
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(authorizationResult.Error);
        }
        
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