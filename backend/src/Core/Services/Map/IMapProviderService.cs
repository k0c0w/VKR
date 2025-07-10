using Domain.Errors;
using ResultMonad;

namespace Services.Map;

public interface IMapProviderService
{
    public Task<Result<BuildingBasementInformation, ErrorMessage>> GetBuildingInformationAsync(Address.Address address,
        CancellationToken cancellationToken);
}