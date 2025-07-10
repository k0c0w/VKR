using System.Text.Json.Serialization;
using Newtonsoft.Json;
using UseCases.Plans.Models;

namespace UseCases.Plans.Update.Models;

public record WallUpdateInstruction : LevelFeatureUpdateInstruction
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public Guid? WallId { get; init; }
    
    [JsonProperty("newGeometry")]
    [JsonPropertyName("newGeometry")]
    public GeometryDto<double[][]>? NewGeometry { get; init; }
}
