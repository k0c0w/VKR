using System.Text.Json.Serialization;
using Newtonsoft.Json;
using UseCases.Plans.Models;

namespace UseCases.Plans.Update.Models;

public record ItEquipmentUpdateInstruction : LevelFeatureUpdateInstruction
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string InventoryNumber { get; init; } = string.Empty;
    
    [JsonProperty("newGeometry")]
    [JsonPropertyName("newGeometry")]
    public GeometryDto<double[]>? NewGeometry { get; init; }
}