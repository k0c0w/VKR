using Common.Extensions;
using Domain.Aggregates;

namespace UseCases.Plans.Models;

public sealed record BuildingPlanItEquipment
{ 
    [Newtonsoft.Json.JsonProperty("relatedToRoomId")]
    [System.Text.Json.Serialization.JsonPropertyName("relatedToRoomId")]
    public long RelatedToRoomId { get; }
    
    [Newtonsoft.Json.JsonProperty("inventoryNumber")]
    [System.Text.Json.Serialization.JsonPropertyName("inventoryNumber")]
    public string InventoryNumber { get; }
    
    [Newtonsoft.Json.JsonProperty("locationPoint")]
    [System.Text.Json.Serialization.JsonPropertyName("locationPoint")]
    public GeometryDto<double[]> LocationPoint { get; private init; }
    
    [Newtonsoft.Json.JsonProperty("name")]
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; private init; }
    
    [Newtonsoft.Json.JsonProperty("cardUrl")]
    [System.Text.Json.Serialization.JsonPropertyName("cardUrl")]
    public string? CardUrl { get; private init; }
    
    [Newtonsoft.Json.JsonProperty("historyUrl")]
    [System.Text.Json.Serialization.JsonPropertyName("historyUrl")]
    public string? HistoryUrl { get; private init; }

    internal BuildingPlanItEquipment(ItEquipment eq)
    {
        InventoryNumber = eq.Id;
        
        LocationPoint = new GeometryDto<double[]>
        {
            Type = eq.Geometry.Type.ToString(),
            Coordinates = eq.Geometry.Coordinates.ToArray(),
        };
        RelatedToRoomId = eq.Description.LocationAudienceCatalogueId;
        Name = eq.Name;
        CardUrl = eq.Description.ItEquipmentCardUrl;
        HistoryUrl = eq.Description.ItEquipmentHistoryUrl;
    }

    [Newtonsoft.Json.JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    protected BuildingPlanItEquipment(string inventoryNumber, long relatedToRoomId, GeometryDto<double[]> locationPoint)
    {
        InventoryNumber = inventoryNumber;
        LocationPoint = locationPoint;
        RelatedToRoomId = relatedToRoomId;
    }
}