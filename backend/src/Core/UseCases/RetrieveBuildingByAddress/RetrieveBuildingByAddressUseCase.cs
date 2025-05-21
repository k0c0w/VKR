using Common.Extensions;
using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;
using Services;
using Services.Map;
using UseCases.Plans.Models;

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
        var addressParsingResult = GetAddress(args);
        if (addressParsingResult.IsFailure)
        {
            return Result.Fail<BuildingDto, ErrorMessage>(addressParsingResult.Error);
        }
        
        var buildingInfoResult = await _mapProviderService.GetBuildingInformationAsync(addressParsingResult.Value!, ct);

        if (buildingInfoResult.IsFailure)
        {
            return Result.Fail<BuildingDto, ErrorMessage>(buildingInfoResult.Error);
        }
   
        var buildingInfo = buildingInfoResult.Value!;
        var building = new BuildingDto
        {
            Geometry = new GeometryDto<double[][][]>
            {
                Type = "Polygon",
                Coordinates = buildingInfo.Geometry.Coordinates
                    .Select(ring => ring.Coordinates
                        .Select(p => p.ToArray())
                        .ToArray())
                    .ToArray(),
            } ,
            LevelsCount = buildingInfo.LevelsCount,
            Address = buildingInfo.Address.ToString(),
        };
        
        return Result.Ok<BuildingDto, ErrorMessage>(building);
    }

    private Result<Address, ErrorMessage> GetAddress(RetrieveBuildingByAddressArgs args)
    {
        var (city, street, house) = args.Address;
        if (!_addressParser.TryParseStreet(street, out var streetType, out var streetName))
        {
            return Result.Fail<Address, ErrorMessage>(ErrorMessage.AddressErrors.CanNotParseStreet);
        }
        if(!_addressParser.TryParseHouse(house, out var houseNumber, out var houseUnit))
        {
            return Result.Fail<Address, ErrorMessage>(ErrorMessage.AddressErrors.CanNotParseHouse);
        }

        return Result.Ok<Address, ErrorMessage>(new Address(
            city: city,
            streetName: streetName,
            streetType: streetType,
            houseNumber: houseNumber,
            houseUnit: houseUnit
        ));
    }
}