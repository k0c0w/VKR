using GeoJSON.Net.Geometry;

namespace Domain.Entities;

public class Wall : GuidEntityBase
{
    public required LineString Geometry { get; init; }
    
    public Wall(Guid id) : base(id) { }
    
    public Wall() {}
}
