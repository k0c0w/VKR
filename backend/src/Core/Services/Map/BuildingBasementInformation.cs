using Domain.ValueObjects;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;

namespace Services.Map;

public class BuildingBasementInformation
{
    public required Address Address { get; init; }
    
    public required uint LevelsCount { get; init; }
    public required Polygon Geometry { get; init; }
}