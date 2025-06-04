using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Room : IHaveIdentity<long>
{
    private readonly Dictionary<string, ItEquipment> _itEquipments;

    public long Id { get; }
    
    internal Level BelongsToLevel { get; }

    public Guid BelongsToLevelId => BelongsToLevel.Id;

    public RoomType Type => RoomDescription.Type;

    public string? Name => RoomDescription.Name;

    public Polygon Geometry => RoomDescription.Geometry;

    public string ArchitectualId => RoomDescription.ArchitectualId;
    
    public IReadOnlyCollection<ItEquipment> ItEquipments => _itEquipments.Values;
    
    private RoomDescription RoomDescription { get; set; }
    
    internal Room (long id, Level belongsToLevel, RoomDescription roomDescription)
    {
        if (id == 0)
        {
            throw new ArgumentException("Default id value met.", nameof(id));
        }
        ArgumentNullException.ThrowIfNull(roomDescription, nameof(roomDescription));

        Id = id;
        RoomDescription = roomDescription;
        BelongsToLevel = belongsToLevel;
        _itEquipments = new Dictionary<string, ItEquipment>();
    }
    
    public ResultWithError<ErrorMessage> AddEquipment(ItEquipment equipment)
    {
        if (_itEquipments.TryAdd(equipment.Id, equipment))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityIsAlreadyExists);
    }

    public static void CreateExistingRoomAtLevel(Level roomLevel, long roomId, RoomDescription description, IEnumerable<ItEquipment> itEquipments)
    {
        var room = new Room(roomId, roomLevel, description);
        foreach (var equipment in itEquipments)
        {
            room.AddEquipment(equipment);
        }
        
        roomLevel.AddStructure(room);
    }
}