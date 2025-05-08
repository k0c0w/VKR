using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Aggregates;

public class Building(BuildingInformation information) : IHaveIdentity<Address>
{
    public Address Id => information.Address;

    public Polygon BasementGeometry => information.Geometry;
}