using System.Text.Json.Serialization;
using Domain.Aggregates;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

public sealed class BuildingPlan
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string? Id { get; private init; }
    
    [JsonPropertyName("region")]
    [JsonProperty("region")]
    public string Region { get; private init; }
    
    [JsonPropertyName("address")]
    [JsonProperty("address")]
    public string Address { get; private init; }
    
    [JsonPropertyName("buildingName")]
    [JsonProperty("buildingName")]
    public string BuildingName { get; private init; }

    [JsonPropertyName("levels")]
    [JsonProperty("levels")]
    public BuildingPlanLevel[] Levels { get; private init; }

    [JsonPropertyName("basementGeometry")]
    [JsonProperty("basementGeometry")]
    public GeometryDto<double[][][]> BasementGeometry { get; private init; }

    [System.Text.Json.Serialization.JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]
    private BuildingPlan(string? id, string region, string address, string buildingName, IEnumerable<BuildingPlanLevel>? levels, GeometryDto<double[][][]> basementGeometry)
    {
        Id = id;
        Region = region;
        Address = address;
        Levels = levels != null ? levels.ToArray() : [];
        BasementGeometry = basementGeometry;
        BuildingName = buildingName;
    }

    internal BuildingPlan(Building building)
    {
        Id = building.Id.ToString();
        Address = building.Location.Address;
        Region = building.Location.Region;
        BasementGeometry = new GeometryDto<double[][][]>
        {
            Coordinates = building.BasementGeometry.Coordinates
                .Select(ring => ring.Coordinates
                    .Select(pos => new[] { pos.Longitude, pos.Latitude })
                    .ToArray())
                .ToArray(),
            Type = building.BasementGeometry.Type.ToString()
        };
        
        Levels = building.Levels
            .OrderBy(x => x.Id.Number)
            .Select(l => new BuildingPlanLevel(l))
            .ToArray();
        BuildingName = building.Name;
    }
}