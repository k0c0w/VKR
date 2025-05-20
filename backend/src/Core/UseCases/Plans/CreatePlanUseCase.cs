using Common.Dto;
using Domain;
using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;
using Services;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class CreatePlanUseCase(
    IAddressParser addressParser,
    IBuildingRepository buildingRepository,
    IUnitOfWork unitOfWork
    )
    : IUseCase<CreatePlanUseCase.CreatePlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    public sealed record CreatePlanUseCaseArgs(BuildingPlan Plan);
    
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(CreatePlanUseCaseArgs args, CancellationToken ct)
    {
        var plan = args.Plan;

        var parseAddressResult = ParseAddress(plan.Address);
        if (parseAddressResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(parseAddressResult.Error);
        }
        var address = parseAddressResult.Value!;

        var notExistenceResult = await BuildingWithSuchAddressDoesNotExistsAsync(address, ct);
        if (notExistenceResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(notExistenceResult.Error);
        }

        if (!plan.Levels.Any())
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.ValidationError("Здание должно содержать хотябы 1 этаж."));
        }
        
        var building = new Building(address, new Polygon(plan.BasementGeometry.Coordinates));
        var addLevelsResult = AddLevels(building, plan.Levels);
        if (addLevelsResult.IsSuccess)
        {
            return await SaveBuildingAsync(building, ct);
        }
        
        return Result.Fail<BuildingPlan, ErrorMessage>(addLevelsResult.Error);
    }

    private async Task<ResultWithError<ErrorMessage>> BuildingWithSuchAddressDoesNotExistsAsync(Address address, CancellationToken ct)
    {
        //todo: add repository method to check existence without loading whole building
        var addressFilter = IBuildingRepository.BuildingFilter.AddressFilter(address);

        var buildingResult = await buildingRepository.GetBuildingAsync(addressFilter, ct);

        if (buildingResult.IsSuccess)
        {
            return ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists);
        }
        
        return buildingResult.Error == ErrorMessage.EntityNotfoundError 
            ? ResultWithError.Ok<ErrorMessage>() 
            : ResultWithError.Fail(buildingResult.Error);
    }
    
    private static ResultWithError<ErrorMessage> AddLevels(Building building, IEnumerable<BuildingPlanLevel> levelPlans)
    {
        var hasLevelWithNumber1 = levelPlans.Any(x => x.Number == 1);
        var withoutLevelWithNumber1 = levelPlans.Where(x => x.Number != 1);

        foreach(var lp in withoutLevelWithNumber1.DistinctBy(x => x.Number))
        {
            var createResult = building.CreateLevel(lp.Number, lp.Name);
            if (createResult.IsFailure)
            {
                return ResultWithError.Fail(createResult.Error);
            }

            var levelResult = building.GetLevel(lp.Number);
            if (levelResult.IsFailure)
            {
                return ResultWithError.Fail(createResult.Error);
            }

            AddEquipments(levelResult.Value!, lp);
        }

        var firstLevelResult = building.GetLevel(1);
        if (firstLevelResult.IsSuccess)
        {
            var firstLevel = firstLevelResult.Value!;
            if (hasLevelWithNumber1)
            {
                var levelPlan = levelPlans.First(x => x.Number == 1);
                AddEquipments(firstLevel, levelPlan);
            }
            else
            {
                var level = building.GetLevel(1);
                if (level.IsSuccess)
                {
                    building.RemoveLevel(level.Value!);
                }
            }
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    }

    private static void AddEquipments(Level level, BuildingPlanLevel levelPlan)
    {
        foreach (var structure in levelPlan.Structure)
        {
            if (structure is BuildingPlanRoom roomPlan)
            {
                var roomDescription = new RoomDescription
                {
                    Geometry = new Polygon(roomPlan.Geometry.Coordinates),
                    Type = roomPlan.Type,
                    ArchitectualId = roomPlan.ArchitectualId,
                    Name = roomPlan.Name,
                };
                level.CreateRoom(roomDescription);
            }
            else if (structure is BuildingPlanWall wallPlan)
            {
                level.CreateWall(new LineString(wallPlan.Geometry.Coordinates));
            } 
        }

        foreach (var itEquipmentPlan in levelPlan.ItEquipments)
        {
            //todo: add itEquipment
        }
    }
    
    private async Task<Result<BuildingPlan, ErrorMessage>> SaveBuildingAsync(Building building, CancellationToken ct)
    {
        try
        {
            unitOfWork.Begin();
            var saveResult = await buildingRepository.AddAsync(building, ct);

            if (saveResult.IsSuccess)
            {
                unitOfWork.Commit();
            }
            else
            {
                unitOfWork.Abort();
            }

            return saveResult.IsSuccess
                ? Result.Ok<BuildingPlan, ErrorMessage>(new BuildingPlan(building))
                : Result.Fail<BuildingPlan, ErrorMessage>(saveResult.Error);
        }
        catch
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.SystemError("Не удалось сохранить здание."));
        }
    }

    private Result<Address, ErrorMessage> ParseAddress(AddressDto addressDto)
    {
        if (!addressParser.TryParseStreet(addressDto.Street, out var streetType, out var streetName))
        {
            return Result.Fail<Address, ErrorMessage>(ErrorMessage.AddressErrors.CanNotParseStreet);
        }

        if (!addressParser.TryParseHouse(addressDto.House, out var houseNumber, out var unitNumber))
        {
            return Result.Fail<Address, ErrorMessage>(ErrorMessage.AddressErrors.CanNotParseHouse);
        }
        
        return Result.Ok<Address, ErrorMessage>(new Address(addressDto.City, streetName, streetType, houseNumber, unitNumber));
    }
}