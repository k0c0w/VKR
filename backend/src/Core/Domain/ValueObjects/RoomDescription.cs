using GeoJSON.Net.Geometry;

namespace Domain.ValueObjects;

public class RoomDescription
{
    public required int LevelNumber { get; init; }
    
    public required RoomType Type { get; init; }
    
    public string? Name { get; init; }
    
    public required Polygon Geometry { get; init; }
}