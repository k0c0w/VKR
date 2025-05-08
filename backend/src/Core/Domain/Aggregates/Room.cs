using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public class Room : IHaveIdentity<string>
{
    private readonly List<ItEquipment> _itEquipments;
    public string Id { get; }

    public required RoomDescription RoomDescription { get; init; }

    public IReadOnlyCollection<ItEquipment> ItEquipments => _itEquipments;

    public Room(string architectualId)
    {
        ArgumentException.ThrowIfNullOrEmpty(architectualId);
        Id = architectualId;
    }
}