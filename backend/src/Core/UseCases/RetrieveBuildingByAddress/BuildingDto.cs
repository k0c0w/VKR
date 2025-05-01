namespace UseCases.RetrieveBuildingByAddress;

public record struct BuildingDto
{
    public Dictionary<string, object> Geometry { get; init; }
    
    public int? LevelsCount { get; init; }
}
