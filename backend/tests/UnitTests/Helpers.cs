using GeoJSON.Net.Geometry;

namespace UnitTests;

internal static class Helpers
{
    public static void AssertGeometryEquality(double[][][] expected, double[][][] actual)
    {
        var a = expected.ToArray();
        var b = actual.ToArray();
        Assert.Equal(a.Length, b.Length);
        for (var i = 0; i < a.Length; i++)
        {
            var expectedLineString = a[i];
            var actualLineString = b[i];
            Assert.Equal(expectedLineString.Length, actualLineString.Length);
            for (var j = 0; j < expectedLineString.Length; j++)
            {
                var coordinates = expectedLineString[i];
                var actualCoordinates = actualLineString[i];
                Assert.True(coordinates.Length == actualCoordinates.Length);
                for (var pos = 0; pos < coordinates.Length; pos++)
                {
                    Assert.Equal(coordinates[pos], actualCoordinates[pos], precision: 7);
                }
            }
        }
    }

    public static double[][][] Unpack(this IEnumerable<LineString> coordinates)
        => coordinates.Select(x => x.Coordinates
                .Select(y => new [] { y.Longitude, y.Latitude })
                .ToArray())
            .ToArray();
}