using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record ElementsJsonModel
{
    [JsonPropertyName("id")]
    public long? Id { get; init; } = null;

    [JsonPropertyName("geometry")]
    public LatLngJsonModel[]? Geometry { get; init; } = null;

    [JsonPropertyName("tags")]
    public Dictionary<string, object> Tags { get; init; } = new();

    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("members")]
    public MemberJsonModel[]? Members { get; init; } = null;
}
