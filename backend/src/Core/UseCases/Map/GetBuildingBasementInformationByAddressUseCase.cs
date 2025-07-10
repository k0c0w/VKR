using Common.Extensions;
using Domain.Errors;
using ResultMonad;
using Services.Address;
using Services.Authorization;
using Services.Map;
using UseCases.Plans.Models;

namespace UseCases.Map;

public sealed class GetBuildingBasementInformationByAddressUseCase(
    IAuthorizationService authService,
    IMapProviderService mapProviderService, 
    IAddressParser addressParser
    )
    : WithAuthorizeUseCaseBase(authService), IUseCase<
        GetBuildingBasementInformationByAddressUseCase.GetBuildingBasementInformationByAddressUseCaseArgs, 
        Result<BuildingBasementInfoDto, ErrorMessage>>
{
    public readonly record struct GetBuildingBasementInformationByAddressUseCaseArgs(string Address);
    
    public async Task<Result<BuildingBasementInfoDto, ErrorMessage>> RunAsync(GetBuildingBasementInformationByAddressUseCaseArgs args
        , CancellationToken ct)
    {
        if (!addressParser.TryParseAddress(args.Address, out var address))
        {
            return Result.Fail<BuildingBasementInfoDto, ErrorMessage>(
                ErrorMessage.ValidationError("Не удалось распарсить адрес."));
        }

        var basementInformationResult = await mapProviderService.GetBuildingInformationAsync(address, ct);
        if (basementInformationResult.IsFailure)
        {
            return Result.Fail<BuildingBasementInfoDto, ErrorMessage>(basementInformationResult.Error);
        }
   
        var buildingInfo = basementInformationResult.Value;
        var building = new BuildingBasementInfoDto
        {
            Geometry = new GeometryDto<double[][][]>
            {
                Type = "Polygon",
                Coordinates = buildingInfo.Geometry.Coordinates
                    .Select(ring => ring.Coordinates
                        .Select(p => p.ToArray())
                        .ToArray())
                    .ToArray(),
            },
        };
        
        return Result.Ok<BuildingBasementInfoDto, ErrorMessage>(building);
    }
}