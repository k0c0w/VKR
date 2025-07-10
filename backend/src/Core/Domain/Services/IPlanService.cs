using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Services;

public interface IPlanService
{
    public Task<Result<Building, ErrorMessage>> CreateNewPlanAsync(User planCreator,
        Location location, string buildingName, Polygon buildingBasement, CancellationToken ct);

    public Task<ResultWithError<ErrorMessage>> DeletePlanAsync(User issuer,
        Guid buildingId, CancellationToken ct);
}