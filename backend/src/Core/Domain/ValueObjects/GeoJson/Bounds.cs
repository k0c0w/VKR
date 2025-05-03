namespace Domain.GeoJson;

public sealed record BoundingBox
{
    public LatLng Min { get; }
    
    public LatLng Max { get; }

    public BoundingBox(LatLng min, LatLng max)
    {
        Min = min;
        Max = max;
    }

    public static implicit operator decimal[][](BoundingBox bb)
    {
        return [
            bb.Min.AsArray(),
            bb.Max.AsArray()
        ];
    }
}