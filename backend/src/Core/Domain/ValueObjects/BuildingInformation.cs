using GeoJSON.Net.Geometry;

namespace Domain.ValueObjects;

public sealed record BuildingInformation
{
    public required Address Address { get; init; }
    
    public required Polygon Geometry { get; init; }

    public uint LevelsCount { get; init; } = 1;
}