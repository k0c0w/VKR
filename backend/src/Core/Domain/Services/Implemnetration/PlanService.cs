using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Services.UserManagementService;

public sealed class PlanService : IPlanService
{
    private IBuildingRepository BuildingRepository { get; }

    public PlanService(IBuildingRepository buildingRepository)
    {
        BuildingRepository = buildingRepository;
    }
    
    public async Task<Result<Building, ErrorMessage>> CreateNewPlanAsync(User planCreator, Location location, string buildingName, Polygon buildingBasement,
        CancellationToken ct)
    {
        if (IssuerIsNotEditor(planCreator))
        {
            return Result.Fail<Building, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var building = new Building(location, buildingName, buildingBasement);
        
        return Result.Ok<Building, ErrorMessage>(building);
    }

    public Task<ResultWithError<ErrorMessage>> DeletePlanAsync(User issuer, Guid buildingId, CancellationToken ct)
    {
        if (IssuerIsNotEditor(issuer))
        {
            var error = ResultWithError.Fail(ErrorMessage.AuthenticationErrors.AccessDenied);
            return Task.FromResult(error);
        }

        var building = Building.CreateExistingBuildingInstance(buildingId, new Location("some location"), "some building",
            new Polygon([[[1, 1], [1, 2], [1, 3], [1, 1]]]), Array.Empty<Level>());
        return BuildingRepository.RemoveAsync(building, ct);
    }
    
    private static bool IssuerIsNotEditor(User issuer)
        => !issuer.Roles.Contains(UserRole.Editor);
}