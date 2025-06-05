namespace Services.EKsu.BuildingCatalogue;


public class Level
{
    public string Name { get; set; }
    public List<Room> Rooms { get; set; } = new List<Room>();
}