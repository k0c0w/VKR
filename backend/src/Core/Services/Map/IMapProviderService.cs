using Domain;
using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;

namespace Services.Map;

public interface IMapProviderService
{
    public Task<Result<BuildingInformation, ErrorMessage>> GetBuildingInformationAsync(Address address,
        CancellationToken cancellationToken);
}