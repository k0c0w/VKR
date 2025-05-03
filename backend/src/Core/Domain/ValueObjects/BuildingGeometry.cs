using Domain.GeoJson;

namespace Domain;

public sealed class BuildingGeometry : Polygon
{
    public BuildingGeometry(LatLng[][] geometry):base(geometry)
    {
        
    }
}