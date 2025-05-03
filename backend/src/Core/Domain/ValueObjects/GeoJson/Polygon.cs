namespace Domain.GeoJson;

public class Polygon : Geometry<decimal[][][]>
{
    public Polygon(LatLng[][] geometry):this(geometry
        .Select(x => x.Select(y => y.AsArray()).ToArray()).ToArray())
    {
    }
    
    public Polygon(decimal[][][] geometry)
    {
        Type = GeometryType.Polygon;

        foreach (var ring in geometry)
        {
            ArgumentNullException.ThrowIfNull(ring, nameof(geometry));

            foreach (var point in ring)
            {
                ArgumentNullException.ThrowIfNull(point, nameof(geometry));
                if (point.Length != 2)
                {
                    throw new ArgumentException("One of points length was out of range.", nameof(geometry));
                }    
            }
        }
        Coordinates = geometry;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Polygon polygon)
        {
            return false;
        }

        if (polygon.Coordinates.Length != Coordinates.Length)
        {
            return false;
        }

        for (var i = 0; i < Coordinates.Length; i++)
        {
            var thisRing = Coordinates[i];
            var thatRing = polygon.Coordinates[i];
            if (thisRing.Length != thatRing.Length)
            {
                return false;
            }

            for (var j = 0; j < thisRing.Length; j++)
            {
                var thisPoint = thisRing[j];
                var thatPoint = thatRing[j];

                if (thatPoint[0] != thisPoint[0] || thatPoint[1] != thisPoint[1])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var ring in Coordinates)
        {
            foreach (var point in ring)
            {
                hashCode ^= (point[0].GetHashCode() ^ point[1].GetHashCode());
            }
        }

        return hashCode;
    }
}