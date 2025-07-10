using GeoJSON.Net.Geometry;

namespace Domain.ValueObjects;

public record RoomDescription
{
    public required RoomType Type { get; init; }
    
    public string? Name { get; init; }
    
    public required Polygon Geometry { get; init; }
}