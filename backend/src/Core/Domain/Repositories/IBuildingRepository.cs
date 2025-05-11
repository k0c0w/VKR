using Domain.Aggregates;
using Domain.Repositories.Common;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IBuildingRepository : IHaveAdd<Building>
{
    Task<BuildingInformation[]> GetAllBuildingInformationAsync(CancellationToken ct);

    Task<Building> GetBuildingAsync(BuildingFilter filter, CancellationToken ct);

    public sealed record BuildingFilter
    {
        public Address? Address { get; }
        public Guid? Id { get; }

        private BuildingFilter(Guid? id, Address? address)
        {
            Id = id;
            Address = address;
        }

        public static BuildingFilter AddressFilter(Address buildingAddress) => new (default, buildingAddress);

        public static BuildingFilter IdFilter(Guid buildingId)
        {
            buildingId.ThrowIfEmpty(nameof(buildingId));

            return new BuildingFilter(buildingId, default);
        }
    }
}