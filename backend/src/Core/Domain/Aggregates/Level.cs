using Domain.Entities;
using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Level : IHaveIdentity<LevelIdentity>
{
    private readonly List<Room> _levelRooms;

    private readonly List<Wall> _levelWalls;
    public LevelIdentity Id { get; }

    public int Number => Id.LevelNumber;
    
    public string Name { get; private set; }

    public IReadOnlyCollection<Room> Rooms => _levelRooms;

    public IReadOnlyCollection<Wall> Walls => _levelWalls;
    
    public Level(Guid buildingId, int number, string? name = null) 
        : this(buildingId, number,  name ?? $"{number} этаж", [], [])
    {
    }
    
    private Level(Guid buildingId, int number, string name, List<Room> rooms,
        List<Wall> walls)
    {
        buildingId.ThrowIfEmpty(nameof(buildingId));
        
        Id = new LevelIdentity(buildingId, number);
        Name = name;
        _levelRooms = rooms;
        _levelWalls = walls;
    }

    public Result<Room, ErrorMessage> CreateRoom(RoomDescription roomDescription)
    {
        var room = new Room(Id, roomDescription, []);

        if (_levelRooms.Any(r => r.ArchitectualId == roomDescription.ArchitectualId))
        {
            return Result.Fail<Room, ErrorMessage>(ErrorMessage.EntityIsAlreadyExists);
        }
        
        _levelRooms.Add(room);

        return Result.Ok<Room, ErrorMessage>(room);
    }
    
    public Result<Wall, ErrorMessage> CreateWall(LineString geometry)
    {
        if (_levelWalls.Any(w => w.Geometry == geometry))
        {
            return Result.Fail<Wall, ErrorMessage>(ErrorMessage.EntityIsAlreadyExists);
        }

        var wall = new Wall(Id, geometry);
        _levelWalls.Add(wall);

        return Result.Ok<Wall, ErrorMessage>(wall);
    }

    public static Level CreateExistingLevel(Guid buildingId, int levelNumber, string levelName, IEnumerable<Room> levelRooms,
        IEnumerable<Wall> levelWalls)
    {
        return new Level(buildingId, levelNumber, levelName, levelRooms.ToList(), levelWalls.ToList());
    }
}