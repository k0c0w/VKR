using UseCases.Plans.Models;

namespace UseCases.Plans.Update.Models;

public sealed record BuildingUpdateInstruction : UpdateInstruction
{
    [System.Text.Json.Serialization.JsonPropertyName("newName")]
    [Newtonsoft.Json.JsonProperty("newName")]
    public string? NewName { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("newGeometry")]
    [Newtonsoft.Json.JsonProperty("newGeometry")]
    public GeometryDto<double[][][]>? NewGeometry { get; init; }
}