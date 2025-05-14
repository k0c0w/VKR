using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;
using Services;
using Services.Map;

namespace UseCases.RetrieveBuildingByAddress;

public sealed record RetrieveBuildingByAddressUseCase 
    : IUseCase<RetrieveBuildingByAddressArgs, Result<BuildingDto, ErrorMessage>>
{
    private readonly IMapProviderService _mapProviderService;
    private readonly IAddressParser _addressParser;
    
    public RetrieveBuildingByAddressUseCase(
        IMapProviderService mapProviderService, 
        IAddressParser addressParser)
    {
        _mapProviderService = mapProviderService;
        _addressParser = addressParser;
    }

    public async Task<Result<BuildingDto, ErrorMessage>> RunAsync(RetrieveBuildingByAddressArgs args, CancellationToken ct)
    {
        var address = GetAddress(args);
        if (address is null)
        {
            return Result.Fail<BuildingDto, ErrorMessage>(new ErrorMessage("Не удалось распарсить адрес."));
        }
        
        var buildingInfoResult = await _mapProviderService.GetBuildingInformationAsync(address, ct);

        if (buildingInfoResult.IsFailure)
        {
            return Result.Fail<BuildingDto, ErrorMessage>(buildingInfoResult.Error);
        }
   
        var buildingInfo = buildingInfoResult.Value!;
        var building = new BuildingDto
        {
            Geometry = buildingInfo.Geometry.Coordinates,
            LevelsCount = buildingInfo.LevelsCount,
            Address = buildingInfo.Address.ToString(),
        };
        
        return Result.Ok<BuildingDto, ErrorMessage>(building);
    }

    private Address? GetAddress(RetrieveBuildingByAddressArgs args)
    {
        var (city, street, house) = args.Address;
        if (!_addressParser.TryParseStreet(street, out var streetType, out var streetName)
            || !_addressParser.TryParseHouse(house, out var houseNumber, out var houseUnit))
        {
            return null;
        }

        return new Address(
            city: city,
            streetName: streetName,
            streetType: streetType,
            houseNumber: houseNumber,
            houseUnit: houseUnit
        );
    }
}