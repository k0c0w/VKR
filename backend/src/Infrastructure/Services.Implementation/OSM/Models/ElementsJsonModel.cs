using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record ElementsJsonModel
{
    [JsonPropertyName("geometry")] public LatLngJsonModel[] Geometry { get; init; } = [];

    [JsonPropertyName("tags")] public TagsJsonModel Tags { get; init; } = new ();

    [JsonPropertyName("type")] public string Type { get; init; } = "";
}