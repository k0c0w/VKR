using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Level : IHaveIdentity<LevelIdentity>
{
    private readonly Dictionary<long, Room> _levelRooms;

    private readonly Dictionary<Guid, Wall> _levelWalls;

    public LevelIdentity Id { get; }

    internal Guid BuildingId => Id.BuildingId;
    
    public string Name { get; protected set; }

    public IReadOnlyCollection<Room> Rooms => _levelRooms.Values;

    public IReadOnlyCollection<Wall> Walls => _levelWalls.Values;

    
    public Level(Guid buildingId, uint number,  string name) 
        : this(new LevelIdentity(buildingId, number),  name, [], [])
    {
    }
    
    private Level(LevelIdentity id, string name, IEnumerable<Room> rooms,
        IEnumerable<Wall> walls)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        Id = id;
        Name = name;
        _levelRooms = rooms.ToDictionary(k => k.Id, v=> v);
        _levelWalls = walls.ToDictionary(k => k.Id, v => v);
    }

    public Result<Room, ErrorMessage> CreateRoom(long id, RoomDescription roomDescription)
    {
        var room = new Room(id, this, roomDescription);

        if (_levelRooms.Values.Any(r => r.Id == id))
        {
            return Result.Fail<Room, ErrorMessage>(ErrorMessage.EntityAlreadyExists);
        }
        
        _levelRooms.Add(room.Id, room);

        return Result.Ok<Room, ErrorMessage>(room);
    }
    
    public async Task<Result<Room, ErrorMessage>> CreateRoomAsync(long id, RoomDescription roomDescription, IRoomRepository roomRepository, CancellationToken ct = default)
    {
        var room = new Room(id, this, roomDescription);

        if (_levelRooms.ContainsKey(room.Id))
        {
            return Result.Fail<Room, ErrorMessage>(ErrorMessage.EntityAlreadyExists);
        }
        
        var roomAddResult = await roomRepository.AddAsync(room, ct);
        if (roomAddResult.IsFailure)
        {
            return Result.Fail<Room, ErrorMessage>(roomAddResult.Error);
        }

        return _levelRooms.TryAdd(room.Id, room) 
            ? Result.Ok<Room, ErrorMessage>(room) 
            : Result.Fail<Room, ErrorMessage>(ErrorMessage.DomainError("Не удалось создать комнату.")); 
    }
    
    public async Task<Result<Wall, ErrorMessage>> CreateWallAsync(LineString geometry, IWallRepository wallRepository, CancellationToken ct = default)
    {
        var wall = new Wall(this, geometry);
        
        var wallAddResult = await wallRepository.AddAsync(wall, ct);
        if (wallAddResult.IsFailure)
        {
            return Result.Fail<Wall, ErrorMessage>(wallAddResult.Error);
        }

        return _levelWalls.TryAdd(wall.Id, wall) 
            ? Result.Ok<Wall, ErrorMessage>(wall) 
            : Result.Fail<Wall, ErrorMessage>(ErrorMessage.DomainError("Не удалось создать стену.")); 
    }
    
    public Result<Wall, ErrorMessage> CreateWall(LineString geometry)
    {
        if (_levelWalls.Values.Any(w => w.Geometry == geometry))
        {
            return Result.Fail<Wall, ErrorMessage>(ErrorMessage.EntityAlreadyExists);
        }

        var wall = new Wall(this, geometry);
        _levelWalls.Add(wall.Id, wall);

        return Result.Ok<Wall, ErrorMessage>(wall);
    }

    internal ResultWithError<ErrorMessage> AddStructure(Room room)
    {
        if (_levelRooms.TryAdd(room.Id, room))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityAlreadyExists);
    }

    internal ResultWithError<ErrorMessage> AddStructure(Wall wall)
    {
        if (_levelWalls.TryAdd(wall.Id, wall))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityAlreadyExists);
    }

    public async Task<Result<Room, ErrorMessage>>  RemoveRoomByIdAsync(long roomId, IRoomRepository roomRepository, CancellationToken ct = default)
    {
        if (_levelRooms.Remove(roomId, out var room))
        {
            var removalResult = await roomRepository.RemoveAsync(room, ct);
            if (removalResult.IsFailure)
            {
                return Result.Fail<Room, ErrorMessage>(removalResult.Error);
            }
            
            room.DetachFromLevel();
            return Result.Ok<Room, ErrorMessage>(room);
        }
        
        return Result.Fail<Room, ErrorMessage>(FormatRoomNotFoundError(roomId));
    }

    public Result<ItEquipment, ErrorMessage> FindItEquipmentById(string inventoryNumber)
    {
        var itEquipment = _levelRooms.Values
            .SelectMany(x => x.ItEquipments)
            .FirstOrDefault(x => x.Id == inventoryNumber);

        return itEquipment is null
            ? Result.Fail<ItEquipment, ErrorMessage>(FormatItEquipmentNotFoundError(inventoryNumber))
            : Result.Ok<ItEquipment, ErrorMessage>(itEquipment);
    }
    
    public Result<Room, ErrorMessage> GetRoom(long roomId)
    {
        return _levelRooms.TryGetValue(roomId, out var room)
            ? Result.Ok<Room, ErrorMessage>(room)
            : Result.Fail<Room, ErrorMessage>(FormatRoomNotFoundError(roomId));
    }
    
    public Result<Wall, ErrorMessage> GetWall(Guid wallId)
    {
        return _levelWalls.TryGetValue(wallId, out var wall)
            ? Result.Ok<Wall, ErrorMessage>(wall)
            : Result.Fail<Wall, ErrorMessage>(FormatWallNotFoundError(wallId));
    }

    public async Task<Result<Wall, ErrorMessage>> RemoveWallByIdAsync(Guid wallId, 
        IWallRepository wallRepository, CancellationToken ct = default)
    {
        if (_levelWalls.Remove(wallId, out var wall))
        {
            var removalResult = await wallRepository.RemoveAsync(wall, ct);

            if (removalResult.IsFailure)
            {
                return Result.Fail<Wall, ErrorMessage>(removalResult.Error);
            }
            
            wall.DetachFromLevel();
            return Result.Ok<Wall, ErrorMessage>(wall);
        }
        
        return Result.Fail<Wall, ErrorMessage>(FormatWallNotFoundError(wallId));
    }
    
    public static Level CreateExistingLevel(Guid buildingId, uint number, string levelName, IEnumerable<Room> levelRooms,
        IEnumerable<Wall> levelWalls)
    {
        return new Level(new LevelIdentity(buildingId, number), levelName, levelRooms.ToList(), levelWalls.ToList());
    }
    
    private ErrorMessage FormatRoomNotFoundError(long roomId) 
        => ErrorMessage.DomainError($"Комната с id {roomId} не найдена на этаже {Id.Number}.");
    
    private ErrorMessage FormatWallNotFoundError(Guid wallId) 
        => ErrorMessage.DomainError($"Стена с id {wallId} не найдена на этаже {Id.Number}.");
    
    private ErrorMessage FormatItEquipmentNotFoundError(string inventoryNumber) 
        => ErrorMessage.DomainError($"ИТ-оборудование {inventoryNumber} не установлено на этаже {Id.Number}.");
}