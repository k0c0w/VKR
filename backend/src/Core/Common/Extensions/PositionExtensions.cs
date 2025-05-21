using GeoJSON.Net.Geometry;

namespace Common.Extensions;

public static class PositionExtensions
{
    public static double[] ToArray(this IPosition position) => [position.Longitude, position.Latitude];
}