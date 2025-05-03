namespace UseCases.RetrieveBuildingByAddress;

public record struct BuildingDto
{
    public decimal[][][] Geometry { get; init; }
    public uint LevelsCount { get; init; }
    
    public string Address { get; init; }
}
