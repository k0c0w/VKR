using UseCases.Plans.Models;

namespace UseCases.Map;

public readonly record struct BuildingBasementInfoDto
{
    public GeometryDto<double[][][]> Geometry { get; init; }
}
