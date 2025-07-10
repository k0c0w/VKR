using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UseCases.Plans.Models;

public record GeometryDto<TArray>
{
    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonProperty("coordinates")]
    [JsonPropertyName("coordinates")]
    public TArray Coordinates { get; set; }
}