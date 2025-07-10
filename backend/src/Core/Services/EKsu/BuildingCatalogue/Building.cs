namespace Services.EKsu.BuildingCatalogue;

public record Building
{
    public string Name { get; set; }
    
    public string Address { get; set; }
    
    public List<Level> Levels { get; set; } = new List<Level>();
}