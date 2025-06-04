using System.Collections.Immutable;
using Common.Dto;
using Domain;
using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;
using Services;
using Services.EKsu;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class CreatePlanUseCase(
    IAuthorizationService authService,
    IAddressParser addressParser,
    IBuildingRepository buildingRepository,
    IUnitOfWork unitOfWork,
    IItEquipmentCatalogue catalogue
    )
    : WithAuthorizeUseCaseBase(authService, Roles), 
        IUseCase<CreatePlanUseCase.CreatePlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    private static readonly ImmutableArray<UserRole> Roles = [..new []{UserRole.Moderator}];
    
    private const string AtLeast1LevelErrorText = "Здание должно содержать хотябы 1 этаж."; 
    private const string UnknownItEquipment = "План содержит оборудование, которое не было добавлено в каталог."; 
    
    public sealed record CreatePlanUseCaseArgs(BuildingPlan Plan);
    
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(CreatePlanUseCaseArgs args, CancellationToken ct)
    {
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(authorizationResult.Error);
        }
        
        var plan = args.Plan;

        var parseAddressResult = ParseAddress(plan.Address);
        if (parseAddressResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(parseAddressResult.Error);
        }
        var address = parseAddressResult.Value!;

        var notExistenceResult = await BuildingWithSuchAddressOrNameDoesNotExistsAsync(address, plan.BuildingName, ct);
        if (notExistenceResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(notExistenceResult.Error);
        }

        var itEquipmentLoadResult = await LoadItEquipmentForBuildingFromCatalogueAsync(args.Plan, ct);
        if (itEquipmentLoadResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(itEquipmentLoadResult.Error);
        }
        var itEquipmentKnowledge = itEquipmentLoadResult.Value!;
        if (!AllItEquipmentIsPresentInCatalogue(plan.Levels.SelectMany(x => x.ItEquipments ?? []),
                itEquipmentKnowledge))
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.ValidationError(UnknownItEquipment));
        }
        
        if (!plan.Levels.Any())
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.ValidationError(AtLeast1LevelErrorText));
        }
        
        var building = new Building(address, args.Plan.BuildingName, new Polygon(plan.BasementGeometry.Coordinates));
        var addLevelsResult = AddLevels(building, plan.Levels, itEquipmentKnowledge);
        if (addLevelsResult.IsSuccess)
        {
            return await SaveBuildingAsync(building, ct);
        }
        
        return Result.Fail<BuildingPlan, ErrorMessage>(addLevelsResult.Error);
    }

    private async Task<Result<IDictionary<string, ItEquipmentDescription>, ErrorMessage>> LoadItEquipmentForBuildingFromCatalogueAsync(
        BuildingPlan buildingPlan, CancellationToken ct)
    {
        var ids = buildingPlan.Levels.SelectMany(l => l.Structure)
            .Where(structure => structure is BuildingPlanRoom)
            .Cast<BuildingPlanRoom>()
            .Select(br => br.Id)
            .ToArray();
        
        var fetchResult = await catalogue.GetAllItEquipmentByRoomIdsAsync(ids, ct);

        if (fetchResult.IsSuccess)
        {
            return Result.Ok<IDictionary<string, ItEquipmentDescription>, ErrorMessage>(fetchResult.Value
                .ToDictionary(k => k.Id, v => v));
        }
        
        return Result.Fail<IDictionary<string, ItEquipmentDescription>, ErrorMessage>(ErrorMessage.ItEquipmentCatalogueErrors.CanNotFetchDataFromCatalogue);
    }
    
    private async Task<ResultWithError<ErrorMessage>> BuildingWithSuchAddressOrNameDoesNotExistsAsync(Address address, string name, CancellationToken ct)
    {
        var existsResult = await buildingRepository.AnyBuildingWithAddressOrNameAsync(name, address, ct);

        return existsResult switch
        {
            { IsSuccess: true, Value: true } => ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists),
            { IsSuccess: true, Value: false } => ResultWithError.Ok<ErrorMessage>(),
            _ => ResultWithError.Fail(existsResult.Error)
        };
    }
    
    private static bool AllItEquipmentIsPresentInCatalogue(IEnumerable<BuildingPlanItEquipment> planItEquipments, 
        IDictionary<string, ItEquipmentDescription> itEquipmentInCatalogue)
    {
        return planItEquipments.All(x => itEquipmentInCatalogue.ContainsKey(x.InventoryNumber));
    }
    
    private static ResultWithError<ErrorMessage> AddLevels(Building building, IEnumerable<BuildingPlanLevel> levelPlans,
        IDictionary<string, ItEquipmentDescription> itEquipmentCatalogue)
    {
        var hasLevelWithNumber1 = levelPlans.Any(x => x.Name == Level.FirstLevelDefaultName);
        var withoutLevelWithNumber1 = levelPlans.Where(x => x.Name != Level.FirstLevelDefaultName);

        foreach(var lp in withoutLevelWithNumber1.DistinctBy(x => x.Name))
        {
            var createResult = building.CreateLevel(lp.Name);
            if (createResult.IsFailure)
            {
                return ResultWithError.Fail(createResult.Error);
            }

            var levelResult = building.GetLevel(lp.Name);
            if (levelResult.IsFailure)
            {
                return ResultWithError.Fail(levelResult.Error);
            }

            AddEquipments(levelResult.Value!, lp, itEquipmentCatalogue);
        }

        var firstLevelResult = building.GetLevel(Level.FirstLevelDefaultName);
        if (firstLevelResult.IsSuccess)
        {
            var firstLevel = firstLevelResult.Value!;
            if (hasLevelWithNumber1)
            {
                var levelPlan = levelPlans.First(x => x.Name == Level.FirstLevelDefaultName);
                AddEquipments(firstLevel, levelPlan, itEquipmentCatalogue);
            }
            else
            {
                var level = building.GetLevel(Level.FirstLevelDefaultName);
                if (level.IsSuccess)
                {
                    building.RemoveLevel(level.Value!);
                }
            }
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    }

    private static void AddEquipments(Level level, BuildingPlanLevel levelPlan, IDictionary<string, ItEquipmentDescription> itEquipmentCatalogue)
    {
        var rooms = new Dictionary<string, Room>();
        foreach (var structure in levelPlan.Structure)
        {
            if (structure is BuildingPlanRoom roomPlan)
            {
                throw new NotImplementedException();
                var roomDescription = new RoomDescription
                {
                    Geometry = new Polygon(roomPlan.Geometry.Coordinates),
                    Type = roomPlan.Type,
                    ArchitectualId = roomPlan.ArchitectualId,
                    Name = roomPlan.Name,
                };
                var roomCreationResult = level.CreateRoom(roomPlan.Id, roomDescription);

                if (roomCreationResult.IsSuccess)
                {
                    rooms.Add(roomCreationResult.Value!.ArchitectualId, roomCreationResult.Value!);
                }
            }
            else if (structure is BuildingPlanWall wallPlan)
            {
                level.CreateWall(new LineString(wallPlan.Geometry.Coordinates));
            } 
        }
        
        foreach (var itEquipmentPlan in levelPlan.ItEquipments)
        {
            var itEquipmentDescription = itEquipmentCatalogue[itEquipmentPlan.InventoryNumber];
            var itEquipment = itEquipmentPlan.ToDomain(itEquipmentDescription);

            if (rooms.TryGetValue(itEquipmentPlan.RelatedToRoomId, out var room))
            {
                room.AddEquipment(itEquipment);
            }
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