using GeoJSON.Net.Geometry;

namespace UseCases.RetrieveBuildingByAddress;

public record struct BuildingDto
{
    public IReadOnlyCollection<LineString> Geometry { get; init; }
    public uint LevelsCount { get; init; }
    
    public string Address { get; init; }
}
