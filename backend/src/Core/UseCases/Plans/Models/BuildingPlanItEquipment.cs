using Common.Extensions;
using Domain.Aggregates;

namespace UseCases.Plans.Models;

public sealed record BuildingPlanItEquipment
{ 
    [Newtonsoft.Json.JsonProperty("relatedToRoomId")]
    [System.Text.Json.Serialization.JsonPropertyName("relatedToRoomId")]
    public string RelatedToRoomId { get; }
    
    [Newtonsoft.Json.JsonProperty("inventoryNumber")]
    [System.Text.Json.Serialization.JsonPropertyName("inventoryNumber")]
    public string InventoryNumber { get; }
    
    [Newtonsoft.Json.JsonProperty("locationPoint")]
    [System.Text.Json.Serialization.JsonPropertyName("locationPoint")]
    public GeometryDto<double[]> LocationPoint { get; private init; }
    
    internal BuildingPlanItEquipment(ItEquipment eq)
    {
        InventoryNumber = eq.Id;
        LocationPoint = new GeometryDto<double[]>
        {
            Type = eq.Geometry.Type.ToString(),
            Coordinates = eq.Geometry.Coordinates.ToArray(),
        };
    }

    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanItEquipment(string inventoryNumber, GeometryDto<double[]> locationPoint)
    {
        InventoryNumber = inventoryNumber;
        LocationPoint = locationPoint;
    }
}