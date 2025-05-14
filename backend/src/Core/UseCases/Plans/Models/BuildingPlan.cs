using System.Text.Json.Serialization;
using Domain.Aggregates;
using Domain.Entities;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

public sealed record BuildingPlan
{
    [JsonProperty("building_id")]
    [JsonPropertyName("building_id")]
    public required Guid BuildingId { get; init; }
    
    [JsonProperty("basement_geometry")]
    [JsonPropertyName("basement_geometry")]
    public required Polygon BasementGeometry { get; init; }

    [JsonProperty("levels")]
    [JsonPropertyName("levels")]
    public required IEnumerable<BuildingPlanLevel> Levels { get; init; }
    private BuildingPlan()
    {
    }

    public static BuildingPlan FromBuilding(Building building)
    {
        return new BuildingPlan
        {
            BuildingId = building.Id,
            BasementGeometry = building.BasementGeometry,
            Levels = building.Levels.Select(BuildingPlanLevel.FromLevel),
        };
    }

    public sealed record BuildingPlanLevel
    {
        [JsonProperty("number")]
        [JsonPropertyName("number")]
        public required int Number { get; init; }
        
        [JsonProperty("name")]
        [JsonPropertyName(("name"))]
        public required string Name { get; init; }

        [JsonProperty("structure")]
        [JsonPropertyName("structure")]
        public required IEnumerable<IGeometryObject> Structure { get; init; } = [];

        [JsonProperty("it_equipments")]
        [JsonPropertyName("it_equipments")]
        public required IEnumerable<BuildingPlanItEquipment> ItEquipments { get; init; } = [];
        
        private BuildingPlanLevel()
        {
        }

        public static BuildingPlanLevel FromLevel(Level level)
        {
            var structure = level.Walls
                .Select(w => w.Geometry)
                .Cast<IGeometryObject>()
                .Concat(level.Rooms.Select(r => r.Geometry));
            
            return new BuildingPlanLevel
            {
                Name = level.Name,
                Number = level.Number,
                Structure = structure,
                ItEquipments = level.Rooms.SelectMany(x => x.ItEquipments)
                    .Select(BuildingPlanItEquipment.FromItEquipment)
            };
        }
    }

    public sealed record BuildingPlanItEquipment
    {
        public static BuildingPlanItEquipment FromItEquipment(ItEquipment itEquipment)
        {
            return new BuildingPlanItEquipment()
            {

            };
        }
    }
};