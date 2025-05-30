using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories.Common;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Repositories;

public interface IBuildingRepository : IHaveAdd<Building>
{
    Task<Result<(Guid BuildingId, Address Address, string BuildingName)[], ErrorMessage>> GetAllBuildingInformationAsync(CancellationToken ct);

    Task<Result<Building, ErrorMessage>> GetBuildingAsync(BuildingFilter filter, CancellationToken ct);

    Task<Result<bool, ErrorMessage>> AnyBuildingWithAddressOrNameAsync(string name, Address address,
        CancellationToken ct);

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