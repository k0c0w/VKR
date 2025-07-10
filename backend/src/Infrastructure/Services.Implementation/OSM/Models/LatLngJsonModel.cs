using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record LatLngJsonModel
{
    [JsonPropertyName("lat")]
    public double Lat { get; init; }

    [JsonPropertyName("lon")]
    public double Lon { get; init; }
}
