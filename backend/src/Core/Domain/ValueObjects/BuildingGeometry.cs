using System.Text.Json.Serialization;
using Domain.GeoJson;

namespace Domain;

public sealed class BuildingGeometry : Polygon
{
    public BuildingGeometry(LatLng[][] geometry):base(geometry)
    {
        
    }
    
    [JsonConstructor]
    protected BuildingGeometry(decimal[][][] coordinates) : base(coordinates){}
}