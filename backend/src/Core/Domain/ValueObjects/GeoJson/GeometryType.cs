using System.Runtime.Serialization;

namespace Domain.GeoJson;

public enum GeometryType
{
    [EnumMember(Value = "Point")]
    Point = 1,
    [EnumMember(Value = "Polyline")]
    Polyline = 2,
    [EnumMember(Value = "Polygon")]
    Polygon = 3,
}
