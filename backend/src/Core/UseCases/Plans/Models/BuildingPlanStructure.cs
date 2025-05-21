using System.Text.Json.Serialization;
using Common.Extensions;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

public abstract record BuildingPlanStructure
{
    public string Id { get; init; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public abstract string Meaning { get; }
}

public record BuildingPlanWall : BuildingPlanStructure
{
    [JsonPropertyName("geometry")]
    [JsonProperty("geometry")]
    public GeometryDto<double[][]> Geometry { get; init; }

    public override string Meaning => "Wall";

    internal BuildingPlanWall(Wall wall)
    {
        Id = wall.Id.ToString();
        Geometry = new GeometryDto<double[][]>
        {
            Type = wall.Geometry.Type.ToString(),
            Coordinates = wall.Geometry.Coordinates
                .Select(x => x.ToArray())
                .ToArray()
        };
    }

    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanWall()
    {
    }
}

public record BuildingPlanRoom : BuildingPlanStructure
{
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("architectualId")]
    [JsonProperty("architectualId")]
    public string ArchitectualId { get; init; }
        
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public RoomType Type { get; init; }
    
    [JsonPropertyName("geometry")]
    [JsonProperty("geometry")]
    public GeometryDto<double[][][]> Geometry { get; init; }

    public override string Meaning => "Room";
    
    internal BuildingPlanRoom(Room room)
    {
        Id = room.Id.ToString();
        Geometry = new GeometryDto<double[][][]>
        {
            Type = room.Geometry.Type.ToString(),
            Coordinates = room.Geometry.Coordinates
                .Select(ring => ring.Coordinates
                    .Select(pos => pos.ToArray())
                    .ToArray()
                )
                .ToArray()
        };
        Name = room.Name ?? "";
        Type = room.Type;
        ArchitectualId = room.ArchitectualId;
    }
}