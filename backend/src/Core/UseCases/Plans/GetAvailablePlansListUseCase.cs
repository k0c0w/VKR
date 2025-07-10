using Domain.Errors;
using Domain.Repositories;
using Services.Authorization;
using ResultMonad;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public sealed class GetAvailablePlansListUseCase(
    IAuthorizationService authorizationService,
    IBuildingRepository buildingRepository)
    : WithAuthorizeUseCaseBase(authorizationService),
      IUseCase<Result<BuildingPlanShortcut[], ErrorMessage>>
{
    public async Task<Result<BuildingPlanShortcut[], ErrorMessage>> RunAsync(CancellationToken ct)
    {
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<BuildingPlanShortcut[], ErrorMessage>(authorizationResult.Error);
        }
        
        var buildingInfosResult = await buildingRepository.GetAllBuildingInformationAsync(ct);
        if (buildingInfosResult.IsFailure)
        {
            return Result.Fail<BuildingPlanShortcut[], ErrorMessage>(buildingInfosResult.Error);
        }

        var allAvailablePlans = buildingInfosResult.Value
            .Select(bi 
                => new BuildingPlanShortcut(bi.BuildingId.ToString(), bi.Location.Region, bi.Location.Address, bi.BuildingName))
            .ToArray();

        return Result.Ok<BuildingPlanShortcut[], ErrorMessage>(allAvailablePlans);
    }
}