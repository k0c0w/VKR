using GeoJSON.Net.Geometry;

namespace Services.Map;

public record BuildingBasementInformation
{
    public required Polygon Geometry { get; init; }
}