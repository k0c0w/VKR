using GeoJSON.Net.Geometry;

namespace UnitTests;

internal static class Helpers
{
    public static void AssertGeometryEquality(IReadOnlyCollection<LineString> expected, IReadOnlyCollection<LineString> actual)
    {
        var a = expected.ToArray();
        var b = actual.ToArray();
        Assert.Equal(a.Length, b.Length);
        for (var i = 0; i < a.Length; i++)
        {
            var expectedLineString = a[i];
            var actualLineString = b[i];
            Assert.Equal(expectedLineString.Coordinates.Count, actualLineString.Coordinates.Count);
            for (var j = 0; j < expectedLineString.Coordinates.Count; j++)
            {
                Assert.Equal(expectedLineString.Coordinates[i].Latitude, actualLineString.Coordinates[i].Latitude, precision: 7);
                Assert.Equal(expectedLineString.Coordinates[j].Longitude, actualLineString.Coordinates[j].Longitude, precision: 7);
            }
        }
    }
}