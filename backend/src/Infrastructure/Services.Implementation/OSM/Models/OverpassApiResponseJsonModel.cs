using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record OverpassApiResponseJsonModel
{
    [JsonPropertyName("elements")]
    public ElementsJsonModel[] Elements { get; init; } = [];
}