using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UseCases.Plans.Update.Models;

public abstract record LevelFeatureUpdateInstruction : UpdateInstruction
{
    [JsonPropertyName("levelNumber")]
    [JsonProperty("levelNumber")]
    public uint LevelNumber { get; init; }
}