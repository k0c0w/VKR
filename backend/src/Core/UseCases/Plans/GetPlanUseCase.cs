using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;
using Services;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class GetPlanUseCase(
    IAddressParser addressParser,
    IBuildingRepository buildingRepository)
    : IUseCase<GetPlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(GetPlanUseCaseArgs args, CancellationToken ct)
    {
        var addressDto = args.BuildingAddress;
        if (!addressParser.TryParseStreet(addressDto.Street, out var streetType, out var streetName))
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(new ErrorMessage("Не удалось распарсить улицу."));
        } 
        if (!addressParser.TryParseHouse(addressDto.House, out var houseNumber, out var houseUnit))
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(new ErrorMessage("Не удалось распарсить дом."));
        }

        var relatedBuildingAddress = new Address(addressDto.City, streetName, streetType, houseNumber, houseUnit);
        var searchFilter = IBuildingRepository.BuildingFilter.AddressFilter(relatedBuildingAddress);
        
        var buildingGetResult = await buildingRepository.GetBuildingAsync(searchFilter, ct);

        if (buildingGetResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(buildingGetResult.Error);
        }

        var planRepresentingBuilding = BuildingPlan.FromBuilding( buildingGetResult.Value!);

        return Result.Ok<BuildingPlan, ErrorMessage>(planRepresentingBuilding);
    }
}