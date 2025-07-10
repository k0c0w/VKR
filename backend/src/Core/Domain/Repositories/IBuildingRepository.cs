using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories.Common;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Repositories;

public interface IBuildingRepository : IHaveAdd<Building>, IHaveUpdate<Building>, IHaveRemove<Building>
{
    Task<Result<(Guid BuildingId, Location Location, string BuildingName)[], ErrorMessage>> GetAllBuildingInformationAsync(CancellationToken ct);

    Task<Result<Building, ErrorMessage>> GetBuildingAsync(BuildingFilter filter, CancellationToken ct);
    
    Task<Result<bool, ErrorMessage>> AnyBuildingWithSameNameAtRegionAsync(string name, string region, CancellationToken ct);

    public sealed record BuildingFilter
    {
        public Guid? Id { get; }

        private BuildingFilter(Guid? id)
        {
            Id = id;
        }

        public static BuildingFilter IdFilter(Guid buildingId)
        {
            buildingId.ThrowIfEmpty(nameof(buildingId));

            return new BuildingFilter(buildingId);
        }
    }
}