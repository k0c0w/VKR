namespace Domain.GeoJson;

public abstract class Geometry<TArray> where TArray : notnull
{
    public GeometryType Type { get; protected set; }
    
    public TArray Coordinates { get; protected set; }
}