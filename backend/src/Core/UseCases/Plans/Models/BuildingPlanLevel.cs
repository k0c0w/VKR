using System.Text.Json.Serialization;
using Domain.Aggregates;
using Newtonsoft.Json;
using UseCases.Plans.Models;

public sealed record BuildingPlanLevel
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonProperty("number")]
    [JsonPropertyName("number")]
    public uint Number { get; init; }

    [JsonProperty("structure")]
    [JsonPropertyName("structure")]
    public IEnumerable<BuildingPlanStructure> Structure { get; init; } = [];

    [JsonProperty("itEquipments")]
    [JsonPropertyName("itEquipments")]
    public IEnumerable<BuildingPlanItEquipment> ItEquipments { get; init; } = [];
    
    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanLevel(
        string name, 
        uint number,
        IEnumerable<BuildingPlanStructure>? structure,  
        IEnumerable<BuildingPlanItEquipment>? itEquipments)
    {
        Number = number;
        Name = name;
        Structure = structure ?? [];
        ItEquipments = itEquipments ?? [];
    }

    internal BuildingPlanLevel(Level level)
    {
        Structure = level.Walls
            .Select(w => new BuildingPlanWall(w))
            .Cast<BuildingPlanStructure>()
            .Concat(level.Rooms.Select(r => new BuildingPlanRoom(r)));

        Name = level.Name;
        Number = level.Id.Number;
        ItEquipments = level.Rooms
            .SelectMany(r => r.ItEquipments.Select(e =>  new BuildingPlanItEquipment(e)));
    }
}