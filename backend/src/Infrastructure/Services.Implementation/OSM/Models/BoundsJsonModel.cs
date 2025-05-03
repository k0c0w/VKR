using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record BoundsJsonModel
{
    [JsonPropertyName("minlat")]
    public decimal MinLat { get; init; }

    [JsonPropertyName("minlon")]
    public decimal MinLon { get; init; }

    [JsonPropertyName("maxlat")]
    public decimal MaxLat { get; init; }

    [JsonPropertyName("maxlon")]
    public decimal MaxLon { get; init; }
}