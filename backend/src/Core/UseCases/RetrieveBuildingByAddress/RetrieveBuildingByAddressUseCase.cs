using Domain;
using Domain.Errors;
using ResultMonad;
using Services.Implementation.OSM;
using Services.Map;

namespace UseCases.RetrieveBuildingByAddress;

public sealed record RetrieveBuildingByAddressUseCase 
    : IUseCase<RetrieveBuildingByAddressDto, Result<BuildingDto, ErrorMessage>>
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

    public async Task<Result<BuildingDto, ErrorMessage>> RunAsync(RetrieveBuildingByAddressDto args, CancellationToken ct)
    {
        var address = GetAddress(args.City, args.Street, args.HouseNumber);
        
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

    private Address GetAddress(string city, string street, string house)
    {
        var (streetType, streetName) = _addressParser.ParseStreet(street);
        var (houseNumber, houseUnit) = _addressParser.ParseHouse(house);

        return new Address(
            city: city,
            streetName: streetName,
            streetType: streetType,
            houseNumber: houseNumber,
            houseUnit: houseUnit
        );
    }
}