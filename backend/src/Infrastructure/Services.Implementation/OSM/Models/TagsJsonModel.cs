using System.Text.Json.Serialization;

namespace Services.Implementation.OSM.Models;

internal sealed record TagsJsonModel
{
    [JsonPropertyName("building:levels")]
    public string? BuildingLevels { get; init; }

    [JsonIgnore]
    public int? LevelCount => int.TryParse(BuildingLevels, out int levels) && levels > 0 ? levels : null;
}