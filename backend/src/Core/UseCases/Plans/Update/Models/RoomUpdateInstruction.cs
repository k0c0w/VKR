using System.Text.Json.Serialization;
using Domain.ValueObjects;
using Newtonsoft.Json;
using UseCases.Plans.Models;

namespace UseCases.Plans.Update.Models;

public sealed record RoomUpdateInstruction : LevelFeatureUpdateInstruction
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long RoomId { get; init; }
    
    [JsonPropertyName("newRoomType")]
    [JsonProperty("newRoomType")]
    public RoomType? NewRoomType { get; init; }
    
    [JsonPropertyName("newGeometry")]
    [JsonProperty("newGeometry")]
    public GeometryDto<double[][][]>? NewGeometry { get; init; }
    
    [JsonPropertyName("newName")]
    [JsonProperty("newName")]
    public string? Name { get; init; }
}