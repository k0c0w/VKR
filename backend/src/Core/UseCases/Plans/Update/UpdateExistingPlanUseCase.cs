using System.Collections.Immutable;
using Domain;
using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;
using Services.Authorization;
using UseCases.Plans.Models;
using UseCases.Plans.Update.Models;

namespace UseCases.Plans.Update;

public class UpdateExistingPlanUseCase(
    IAuthorizationService authService,
    IBuildingRepository buildingRepository,
    IRoomRepository roomRepository,
    IWallRepository wallRepository,
    IItEquipmentRepository itEquipmentRepository,
    IUnitOfWork unitOfWork
) : WithAuthorizeUseCaseBase(authService, Roles),
    IUseCase<UpdateExistingPlanUseCase.UpdateBuildingUseCaseArgs, Result<BuildingPlan, ErrorMessage>>
{
    private static readonly ImmutableArray<UserRole> Roles = [UserRole.Editor];

    public sealed record UpdateBuildingUseCaseArgs(Guid BuildingId, IEnumerable<UpdateInstruction> Instructions);

    public async Task<Result<BuildingPlan, ErrorMessage>> RunAsync(UpdateBuildingUseCaseArgs args, CancellationToken ct)
    {
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(authorizationResult.Error);
        }

        var buildingResult =
            await buildingRepository.GetBuildingAsync(IBuildingRepository.BuildingFilter.IdFilter(args.BuildingId), ct);
        if (buildingResult.IsFailure)
        {
            return Result.Fail<BuildingPlan, ErrorMessage>(buildingResult.Error);
        }

        var building = buildingResult.Value;
        await unitOfWork.BeginAsync();

        foreach (var instruction in args.Instructions)
        {
            Task<ResultWithError<ErrorMessage>> actionTask;
            if (instruction is BuildingUpdateInstruction buildingUpdateInstruction)
            {
                actionTask = ApplyInstructionAsync(building, buildingUpdateInstruction, ct);
            } 
            else if (instruction is LevelFeatureUpdateInstruction levelFeatureUpdateInstruction)
            {
                actionTask = ApplyInstructionAsync(building, levelFeatureUpdateInstruction, ct);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unknown update type, forgot to add handler?",
                    new InvalidOperationException());
            }
            
            var result = await actionTask;
            if (result.IsFailure)
            {
                await unitOfWork.RollbackAsync();
                return Result.Fail<BuildingPlan, ErrorMessage>(result.Error);
            }
        }

        await unitOfWork.CommitAsync();
        return Result.Ok<BuildingPlan, ErrorMessage>(new BuildingPlan(building));
    }

    private async Task<ResultWithError<ErrorMessage>> ApplyInstructionAsync(Building building,
        BuildingUpdateInstruction instruction, CancellationToken ct)
    {
        if (instruction.UpdateAction == UpdateInstruction.UpdateActionEnum.Create)
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Здание нельзя создать."));
        }

        if (instruction.UpdateAction == UpdateInstruction.UpdateActionEnum.Delete)
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Здание нельзя удалить."));
        }

        if (!string.IsNullOrEmpty(instruction.NewName))
        {
            var changeNameResult = await building.ChangeNameAsync(instruction.NewName, buildingRepository, ct);
            if (changeNameResult.IsFailure)
            {
                return changeNameResult;
            }
        }

        if (instruction.NewGeometry is not null)
        {
            if (instruction.NewGeometry.Type != nameof(Polygon))
            {
                return ResultWithError.Fail(
                    ErrorMessage.ValidationError("Основание здания должно быть представлено полигоном."));
            }

            var newGeometry = new Polygon(instruction.NewGeometry.Coordinates.Select(ring =>
                new LineString(ring.Select(coord => new Position(coord[1], coord[0])))));
            var update = await building.UpdateBasementGeometry(newGeometry, buildingRepository, ct);

            if (update.IsFailure)
            {
                return update;
            }
        }

        return ResultWithError.Ok<ErrorMessage>();
    }

    private Task<ResultWithError<ErrorMessage>> ApplyInstructionAsync(Building building,
        LevelFeatureUpdateInstruction instruction, CancellationToken ct)
    {
        var levelGetResult = building.GetLevel(instruction.LevelNumber);
        if (levelGetResult.IsFailure)
        {
            return Task.FromResult(ResultWithError.Fail(levelGetResult.Error));
        }

        var level = levelGetResult.Value;

        return instruction.UpdateType switch
        {
            UpdateInstruction.UpdateInstructionType.RoomUpdate => ApplyInstructionAsync(level, (RoomUpdateInstruction)instruction, ct),
            UpdateInstruction.UpdateInstructionType.WallUpdate => ApplyInstructionAsync(level, (WallUpdateInstruction)instruction, ct),
            UpdateInstruction.UpdateInstructionType.ItEquipmentUpdate => ApplyInstructionAsync(level, 
                (ItEquipmentUpdateInstruction)instruction, ct),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private Task<ResultWithError<ErrorMessage>> ApplyInstructionAsync(Level level,
        ItEquipmentUpdateInstruction instruction, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(instruction.InventoryNumber))
        {
            return Task.FromResult(ResultWithError.Fail(ErrorMessage.ValidationError("Инвентарный номер ИТ-борудования не указан.")));
        }

        if (instruction.UpdateType != UpdateInstruction.UpdateInstructionType.ItEquipmentUpdate)
        {
            // ski[ this updates
            return Task.FromResult(ResultWithError.Ok<ErrorMessage>());
        }

        return UpdateItEquipmentAsync(level, instruction, ct);
    }

    private async Task<ResultWithError<ErrorMessage>> UpdateItEquipmentAsync(Level level,
        ItEquipmentUpdateInstruction instruction, CancellationToken ct)
    {
        if (instruction.NewGeometry is null || instruction.NewGeometry.Type != nameof(Point))
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Указана неверная геометрия ИТ-оборудования."));
        }
        var itEquipmentFindResult = level.FindItEquipmentById(instruction.InventoryNumber);
        if (itEquipmentFindResult.IsFailure)
        {
            return ResultWithError.Fail(itEquipmentFindResult.Error);
        }

        var itEquipment = itEquipmentFindResult.Value;
        
        var pos = new Position(latitude: instruction.NewGeometry.Coordinates[1], longitude: instruction.NewGeometry.Coordinates[0]);
        
        var update = await itEquipment.MoveToPositionAsync(pos, itEquipmentRepository, ct);
        return update.IsFailure ? ResultWithError.Fail(update.Error) : ResultWithError.Ok<ErrorMessage>();
    }
    
    private Task<ResultWithError<ErrorMessage>> ApplyInstructionAsync(Level level,
        WallUpdateInstruction instruction, CancellationToken ct)
    {
        return instruction.UpdateAction switch
        {
            UpdateInstruction.UpdateActionEnum.Delete => DeleteWallAsync(level, instruction, ct),
            UpdateInstruction.UpdateActionEnum.Create => CreateWallAsync(level, instruction, ct),
            UpdateInstruction.UpdateActionEnum.Update => UpdateWallAsync(level, instruction, ct),
            _ => throw new ArgumentOutOfRangeException(
                $"Unknown {nameof(instruction.UpdateAction)}, forgot to add handler?")
        };
    }
    
    private async Task<ResultWithError<ErrorMessage>> CreateWallAsync(Level level, WallUpdateInstruction instruction,
        CancellationToken ct)
    {
        if (instruction is not { NewGeometry: not null })
        {
            return ResultWithError.Fail(ErrorMessage
                .ValidationError("Не достаточно параметров для создания стены."));
        }

        if (instruction.NewGeometry.Type != nameof(LineString))
        {
            return ResultWithError.Fail(
                ErrorMessage.ValidationError("Геометрией для комнаты может быть только линия."));
        }

        var geometry =
            new LineString(instruction.NewGeometry.Coordinates.Select(coord => new Position(coord[1], coord[0])));

        var createWallResult = await level.CreateWallAsync(geometry, wallRepository, ct);
        return createWallResult.IsFailure
            ? ResultWithError.Fail(createWallResult.Error)
            : ResultWithError.Ok<ErrorMessage>();
    }

    private async Task<ResultWithError<ErrorMessage>> DeleteWallAsync(Level level, WallUpdateInstruction instruction,
        CancellationToken ct)
    {
        if (instruction is { WallId: null })
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Не указан индетефикатор стены."));
        }

        var removalResult = await level.RemoveWallByIdAsync(instruction.WallId.Value, wallRepository, ct);

        return removalResult.IsSuccess
            ? ResultWithError.Ok<ErrorMessage>()
            : ResultWithError.Fail(removalResult.Error);
    }

    private async Task<ResultWithError<ErrorMessage>> UpdateWallAsync(Level level, WallUpdateInstruction instruction,
        CancellationToken ct)
    {
        if (instruction is not { WallId: not null, NewGeometry: not null })
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Не достаточно параметров для обновления стены."));
        }

        var wallGetResult = level.GetWall(instruction.WallId.Value);
        if (wallGetResult.IsFailure)
        {
            return ResultWithError.Fail(wallGetResult.Error);
        }

        var wall = wallGetResult.Value;
        if (instruction.NewGeometry.Type != nameof(LineString))
        {
            return ResultWithError.Fail(
                ErrorMessage.ValidationError("Геометрией для комнаты может быть только линия."));
        }

        var geometry =
            new LineString(instruction.NewGeometry.Coordinates.Select(coord => new Position(coord[1], coord[0])));
        var updateResult = await wall.UpdateGeometryAsync(geometry, wallRepository, ct);

        return updateResult.IsFailure ? updateResult : ResultWithError.Ok<ErrorMessage>();
    }
    
    private Task<ResultWithError<ErrorMessage>> ApplyInstructionAsync(Level level,
        RoomUpdateInstruction instruction, CancellationToken ct)
    {
        return instruction.UpdateAction switch
        {
            UpdateInstruction.UpdateActionEnum.Delete => DeleteRoomAsync(level, instruction, ct),
            UpdateInstruction.UpdateActionEnum.Create => CreateRoomAsync(level, instruction, ct),
            UpdateInstruction.UpdateActionEnum.Update => UpdateRoomAsync(level, instruction, ct),
            _ => throw new ArgumentOutOfRangeException(
                $"Unknown {nameof(instruction.UpdateAction)}, forgot to add handler?")
        };
    }
    
    private async Task<ResultWithError<ErrorMessage>> CreateRoomAsync(Level level, RoomUpdateInstruction instruction,
        CancellationToken ct)
    {
        if (instruction is not { NewRoomType: not null, NewGeometry: not null })
        {
            return ResultWithError.Fail(ErrorMessage
                .ValidationError($"Не достаточно параметров для создания комнаты с id {instruction.RoomId}."));
        }

        if (instruction.NewGeometry.Type != nameof(Polygon))
        {
            return ResultWithError.Fail(
                ErrorMessage.ValidationError("Геометрией для комнаты может быть только полигон."));
        }

        var geometry = new Polygon(instruction.NewGeometry.Coordinates.Select(ring =>
            new LineString(ring.Select(coord => new Position(coord[1], coord[0])))));

        var roomDescriptor = new RoomDescription
        {
            Geometry = geometry,
            Name = instruction.Name,
            Type = instruction.NewRoomType.Value,
        };
        var roomCreateResult = await level.CreateRoomAsync(instruction.RoomId, roomDescriptor, roomRepository, ct);

        if (roomCreateResult.IsFailure)
        {
            return ResultWithError.Fail(roomCreateResult.Error);
        }

        return ResultWithError.Ok<ErrorMessage>();
    }

    private async Task<ResultWithError<ErrorMessage>> DeleteRoomAsync(Level level, RoomUpdateInstruction instruction,
        CancellationToken ct)
    {
        var removalResult = await level.RemoveRoomByIdAsync(instruction.RoomId, roomRepository, ct);

        return removalResult.IsSuccess
            ? ResultWithError.Ok<ErrorMessage>()
            : ResultWithError.Fail(removalResult.Error);
    }

    private async Task<ResultWithError<ErrorMessage>> UpdateRoomAsync(Level level, RoomUpdateInstruction instruction,
        CancellationToken ct)
    {
        var roomGetResult = level.GetRoom(instruction.RoomId);
        if (roomGetResult.IsFailure)
        {
            return ResultWithError.Fail(roomGetResult.Error);
        }

        var room = roomGetResult.Value;
        if (instruction.NewGeometry is not null)
        {
            if (instruction.NewGeometry.Type != nameof(Polygon))
            {
                return ResultWithError.Fail(
                    ErrorMessage.ValidationError("Геометрией для комнаты может быть только полигон."));
            }

            var geometry = new Polygon(instruction.NewGeometry.Coordinates);
            var geometryUpdate = await room.UpdateGeometryAsync(geometry, roomRepository, itEquipmentRepository, ct);
            if (geometryUpdate.IsFailure)
            {
                return geometryUpdate;
            }
        }

        if (!instruction.NewRoomType.HasValue)
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        var updateType = await room.ChangeTypeAsync(instruction.NewRoomType.Value, roomRepository, ct);

        return updateType.IsFailure ? updateType : ResultWithError.Ok<ErrorMessage>();
    }
}