using Domain.Entities;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Aggregates;

public class Room : IHaveIdentity<Guid>
{
    private readonly List<ItEquipment> _itEquipments = new ();
    public Guid Id { get; }
    
    public IReadOnlyCollection<ItEquipment> ItEquipments => _itEquipments;

    public LevelIdentity BelongsToLevel { get; }

    public RoomType Type => RoomDescription.Type;

    public string? Name => RoomDescription.Name;

    public Polygon Geometry => RoomDescription.Geometry;

    public string ArchitectualId => RoomDescription.ArchitectualId;
    
    private RoomDescription RoomDescription { get; set; }
    
    internal Room(LevelIdentity belongsToLevel, RoomDescription roomDescription, IEnumerable<ItEquipment> equipments) 
        : this(Guid.CreateVersion7(), belongsToLevel, roomDescription)
    {
        _itEquipments = equipments.ToList();
    }

    private Room (Guid id, LevelIdentity levelIdentity, RoomDescription roomDescription)
    {
        id.ThrowIfEmpty(nameof(id));
        ArgumentNullException.ThrowIfNull(roomDescription, nameof(roomDescription));

        Id = id;
        RoomDescription = roomDescription;
        BelongsToLevel = levelIdentity;
    }

    public static Room CreateExistingButEmptyRoom(Guid roomId, LevelIdentity roomLevel, RoomDescription description)
    {
        return new Room(roomId, roomLevel, description);
    }
}