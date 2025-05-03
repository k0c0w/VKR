using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record LatLngJsonModel
{
    [JsonPropertyName("lat")]
    public decimal Lat { get; init; }

    [JsonPropertyName("lon")]
    public decimal Lng { get; init; }
}