using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Services.PlanImageAnalyzer;

public record RoomOnImage
{
    [JsonPropertyName("text")]
    [JsonProperty("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("mask")]
    [JsonProperty("mask")]
    public required string Base64EncodedMask { get; init; }
    
    [JsonPropertyName("bbox")]
    [JsonProperty("bbox")]
    public required int[] BoundingBox { get; init; }
}