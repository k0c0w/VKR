using Domain.Entities;
using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Level : IHaveIdentity<Guid>
{
    public const string FirstLevelDefaultName = "1 этаж";
    
    private readonly Dictionary<Guid, Room> _levelRooms;

    private readonly Dictionary<Guid, Wall> _levelWalls;

    private readonly Dictionary<string, ItEquipment> _levelItEquipments;
    
    public Guid Id { get; }
    
    internal Guid BuildingId { get; }
    
    public string Name { get; protected set; }

    public IReadOnlyCollection<Room> Rooms => _levelRooms.Values;

    public IReadOnlyCollection<Wall> Walls => _levelWalls.Values;

    public IReadOnlyCollection<ItEquipment> ItEquipments => _levelItEquipments.Values;
    
    public Level(Guid buildingId, string name) 
        : this(Guid.CreateVersion7(),  buildingId,  name, [], [], [])
    {
    }
    
    private Level(Guid id, Guid buildingId, string name, IEnumerable<Room> rooms,
        IEnumerable<Wall> walls, Dictionary<string, ItEquipment> itEquipments)
    {
        buildingId.ThrowIfEmpty(nameof(buildingId));
        id.ThrowIfEmpty(nameof(id));
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        Id = id;
        BuildingId = buildingId;
        Name = name;
        _levelRooms = rooms.ToDictionary(k => k.Id, v=> v);
        _levelWalls = walls.ToDictionary(k => k.Id, v => v);
        _levelItEquipments = itEquipments;
    }

    public Result<Room, ErrorMessage> CreateRoom(RoomDescription roomDescription)
    {
        var room = new Room(this, roomDescription);

        if (_levelRooms.Values.Any(r => r.ArchitectualId == roomDescription.ArchitectualId))
        {
            return Result.Fail<Room, ErrorMessage>(ErrorMessage.EntityIsAlreadyExists);
        }
        
        _levelRooms.Add(room.Id, room);

        return Result.Ok<Room, ErrorMessage>(room);
    }
    
    public Result<Wall, ErrorMessage> CreateWall(LineString geometry)
    {
        if (_levelWalls.Values.Any(w => w.Geometry == geometry))
        {
            return Result.Fail<Wall, ErrorMessage>(ErrorMessage.EntityIsAlreadyExists);
        }

        var wall = new Wall(this, geometry);
        _levelWalls.Add(wall.Id, wall);

        return Result.Ok<Wall, ErrorMessage>(wall);
    }

    public ResultWithError<ErrorMessage> AddEquipment(ItEquipment equipment)
    {
        if (_levelItEquipments.TryAdd(equipment.Id, equipment))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists);
    }

    internal ResultWithError<ErrorMessage> AddStructure(Room room)
    {
        if (_levelRooms.TryAdd(room.Id, room))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists);
    }

    internal ResultWithError<ErrorMessage> AddStructure(Wall wall)
    {
        if (_levelWalls.TryAdd(wall.Id, wall))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists);
    }
    
    public static Level CreateExistingLevel(Guid id, Guid buildingId, string levelName, IEnumerable<Room> levelRooms,
        IEnumerable<Wall> levelWalls, IEnumerable<ItEquipment> itEquipments)
    {
        return new Level(id, buildingId, levelName, levelRooms.ToList(), levelWalls.ToList(), itEquipments
            .ToDictionary(k => k.Id, v=>v));
    }
}