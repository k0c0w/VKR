namespace Domain.GeoJson;

public readonly struct LatLng
{
    public decimal Lat { get; init; }

    public decimal Lng { get; init; }
    
    public override string ToString()
    {
        return $"[{Lng}; {Lat}]";
    }

    public decimal[] AsArray() => [Lng, Lat];
}