using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;

namespace Services.Map;

public interface IMapProviderService
{
    public Task<Result<BuildingBasementInformation, ErrorMessage>> GetBuildingInformationAsync(Address address,
        CancellationToken cancellationToken);
}