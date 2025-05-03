namespace Domain;

public sealed record BuildingInformation
{
    public Address Address { get; init; }
    
    public BuildingGeometry Geometry { get; init; }
    
    public uint LevelsCount { get; init; }
}