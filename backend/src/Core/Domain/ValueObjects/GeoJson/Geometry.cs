using System.Text.Json.Serialization;

namespace Domain.GeoJson;

public abstract class Geometry<TArray> where TArray : notnull
{
    [JsonConstructor]
    protected Geometry()
    {
    }
    public GeometryType Type { get; protected set; }
    
    public TArray Coordinates { get; protected set; }
}