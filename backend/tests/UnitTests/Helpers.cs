namespace UnitTests;

internal static class Helpers
{
    public static void AssertGeometryEquality(decimal[][][] expected, decimal[][][] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Length, actual[i].Length);
            for (var j = 0; j < expected[i].Length; j++)
            {
                Assert.Equal(expected[i][j][0], actual[i][j][0]);
                Assert.Equal(expected[i][j][1], actual[i][j][1]);
            }
        }
    }
}