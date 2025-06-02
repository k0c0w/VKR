using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Aggregates;

public class Room : IHaveIdentity<Guid>
{
    public Guid Id { get; }
    
    internal Level BelongsToLevel { get; }

    public Guid BelongsToLevelId => BelongsToLevel.Id;

    public RoomType Type => RoomDescription.Type;

    public string? Name => RoomDescription.Name;

    public Polygon Geometry => RoomDescription.Geometry;

    public string ArchitectualId => RoomDescription.ArchitectualId;
    
    private RoomDescription RoomDescription { get; set; }
    
    internal Room(Level belongsToLevel, RoomDescription roomDescription) 
        : this(Guid.CreateVersion7(), belongsToLevel, roomDescription)
    {
    }

    private Room (Guid id, Level belongsToLevel, RoomDescription roomDescription)
    {
        id.ThrowIfEmpty(nameof(id));
        ArgumentNullException.ThrowIfNull(roomDescription, nameof(roomDescription));

        Id = id;
        RoomDescription = roomDescription;
        BelongsToLevel = belongsToLevel;
    }

    public static void CreateExistingRoomAtLevel(Level roomLevel, Guid roomId, RoomDescription description)
    {
        var room = new Room(roomId, roomLevel, description);
        roomLevel.AddStructure(room);
    }
}