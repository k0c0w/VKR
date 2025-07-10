using System.Collections.Immutable;
using System.Transactions;
using Domain;
using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using ResultMonad;
using Services.Authorization;
using Services.EKsu;
using UseCases.Plans.Models;

namespace UseCases.Plans;

public class CreatePlanUseCase(
    IPlanService planService,
    IAuthorizationService authService,
    IBuildingRepository buildingRepository,
    IUnitOfWork unitOfWork,
    IItEquipmentCatalogue catalogue,
    ILogger<CreatePlanUseCase>? logger = default
    )
    : WithAuthorizeUseCaseBase(authService, Roles), 
        IUseCase<CreatePlanUseCase.CreatePlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    private static readonly ImmutableArray<UserRole> Roles = [UserRole.Editor];
    
    private const string AtLeast1LevelErrorText = "Здание должно содержать хотябы 1 этаж."; 
    
    public sealed record CreatePlanUseCaseArgs(BuildingPlan Plan);
    
    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(CreatePlanUseCaseArgs args, CancellationToken ct)
    {
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(authorizationResult.Error);
        }

        var plan = args.Plan;

        if (!(string.IsNullOrEmpty(plan.Address) || IsAddressValid(plan.Address)))
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.ValidationError("Не верный формат адреса."));
        }

        var notExistenceResult = await BuildingWithSuchNameDoesNotExistsAtRegionAsync(plan.BuildingName, plan.Region, ct);
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
        
        if (!plan.Levels.Any())
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.ValidationError(AtLeast1LevelErrorText));
        }

        var buildingCreateResult = await CreateBuildingAsync(plan, ct);
        if (buildingCreateResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(buildingCreateResult.Error);
        }

        var building = buildingCreateResult.Value;
        var addLevelsResult = AddLevels(building, plan.Levels, itEquipmentKnowledge);
        if (addLevelsResult.IsSuccess)
        {
            return await SaveBuildingAsync(building, ct);
        }
        
        return Result.Fail<BuildingPlan, ErrorMessage>(addLevelsResult.Error);
    }

    private async Task<Result<Building, ErrorMessage>> CreateBuildingAsync(BuildingPlan plan, CancellationToken ct)
    {
        var buildingLocation = new Location(plan.Region, plan.Address);
        var basement = new Polygon(plan.BasementGeometry.Coordinates);
        var currentUser = await GetCurrentUserAsync();

        var buildingCreationResult =
            await planService.CreateNewPlanAsync(currentUser, buildingLocation, plan.BuildingName, basement, ct);

        return buildingCreationResult;
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
    
    private async Task<ResultWithError<ErrorMessage>> BuildingWithSuchNameDoesNotExistsAtRegionAsync(string name, string region, CancellationToken ct)
    {
        var existsResult = await buildingRepository.AnyBuildingWithSameNameAtRegionAsync(name, region, ct);

        return existsResult switch
        {
            { IsSuccess: true, Value: true } => ResultWithError.Fail(ErrorMessage.EntityAlreadyExists),
            { IsSuccess: true, Value: false } => ResultWithError.Ok<ErrorMessage>(),
            _ => ResultWithError.Fail(existsResult.Error)
        };
    }
    
    private static IEnumerable<BuildingPlanItEquipment>  FilterByPresenceInCatalogue(IEnumerable<BuildingPlanItEquipment> planItEquipments, 
        IDictionary<string, ItEquipmentDescription> itEquipmentInCatalogue)
    {
        return planItEquipments
            .Where(x => itEquipmentInCatalogue.ContainsKey(x.InventoryNumber));
    }
    
    private static ResultWithError<ErrorMessage> AddLevels(Building building, IEnumerable<BuildingPlanLevel> levelPlans,
        IDictionary<string, ItEquipmentDescription> itEquipmentCatalogue)
    {
        foreach(var lp in levelPlans)
        {
            var createResult = building.CreateLevel(lp.Name);
            if (createResult.IsFailure)
            {
                return ResultWithError.Fail(createResult.Error);
            }

            var level = createResult.Value;
            AddEquipments(level, lp, itEquipmentCatalogue);
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    }

    private static void AddEquipments(Level level, BuildingPlanLevel levelPlan, IDictionary<string, ItEquipmentDescription> itEquipmentCatalogue)
    {
        var rooms = new Dictionary<long, Room>();
        foreach (var structure in levelPlan.Structure)
        {
            if (structure is BuildingPlanRoom roomPlan)
            {
                var roomDescription = new RoomDescription
                {
                    Geometry = new Polygon(roomPlan.Geometry.Coordinates),
                    Type = roomPlan.Type,
                    Name = roomPlan.Name,
                };
                var roomCreationResult = level.CreateRoom(roomPlan.Id, roomDescription);

                if (roomCreationResult.IsSuccess)
                {
                    rooms.Add(roomCreationResult.Value.Id, roomCreationResult.Value!);
                }
            }
            else if (structure is BuildingPlanWall wallPlan)
            {
                level.CreateWall(new LineString(wallPlan.Geometry.Coordinates));
            } 
        }
        
        foreach (var itEquipmentPlan in FilterByPresenceInCatalogue(levelPlan.ItEquipments, itEquipmentCatalogue))
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
            await unitOfWork.BeginAsync();
            var saveResult = await buildingRepository.AddAsync(building, ct);

            if (saveResult.IsSuccess)
            {
               await unitOfWork.CommitAsync();
            }
            else
            {
                await unitOfWork.RollbackAsync();
            }

            return saveResult.IsSuccess
                ? Result.Ok<BuildingPlan, ErrorMessage>(new BuildingPlan(building))
                : Result.Fail<BuildingPlan, ErrorMessage>(saveResult.Error);
        }
        catch (TransactionAbortedException)
        {
            return  Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.DomainError("Транзакция была прервана."));
        }
        catch(Exception ex)
        {
            logger?.LogDebug(ex, "Exception during creation of plan: {message}.", ex.Message);
            
            await unitOfWork.RollbackAsync();
            return Result.Fail<BuildingPlan, ErrorMessage>(ErrorMessage.SystemError("Не удалось сохранить здание."));
        }
    }

    private static bool IsAddressValid(string address)
    {
        //todo: address validation
        return true;
    }
}