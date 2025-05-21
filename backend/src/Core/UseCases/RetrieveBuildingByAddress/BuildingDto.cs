using UseCases.Plans.Models;

namespace UseCases.RetrieveBuildingByAddress;

public record struct BuildingDto
{
    public GeometryDto<double[][][]> Geometry { get; init; }
    public uint LevelsCount { get; init; }
    
    public string Address { get; init; }
}
