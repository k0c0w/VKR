using System.Text.Json.Serialization;
using Common.Extensions;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Meaning))]
[JsonDerivedType(typeof(BuildingPlanWall), "Wall")]
[JsonDerivedType(typeof(BuildingPlanRoom), "Room")]
[Newtonsoft.Json.JsonConverter(typeof(BuildingPlanStructureConverter))]
public abstract record BuildingPlanStructure
{
    public string? Id { get; init; }
    
    [JsonProperty("meaning")]
    [JsonPropertyName("meaning")]
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
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; init; }
    
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public RoomType Type { get; init; }
    
    [JsonPropertyName("geometry")]
    [JsonProperty("geometry")]
    public GeometryDto<double[][][]> Geometry { get; init; }

    [JsonPropertyName("meaning")]
    [JsonProperty("meaning")]
    public override string Meaning => "Room";
    
    internal BuildingPlanRoom(Room room)
    {
        Id = room.Id;
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
    }

    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanRoom()
    {
    }
}