using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record MemberJsonModel
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("ref")]
    public long Ref { get; init; } = 0;

    [JsonPropertyName("role")]
    public string Role { get; init; } = "";

    [JsonPropertyName("geometry")]
    public LatLngJsonModel[]? Geometry { get; init; } = null;
}