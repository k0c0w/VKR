namespace Services.Implementation.OSM;

public record MapServiceConfiguration
{
    public string OverpassApiHost { get; init; } = "";
}