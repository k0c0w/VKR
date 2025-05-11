using GeoJSON.Net.Geometry;

namespace Domain.ValueObjects;

public sealed record BuildingInformation
{
    public required Address Address { get; init; }
    
    public required Polygon Geometry { get; init; }

    private uint _levelsCount = 1;

    public uint LevelsCount
    {
        get => _levelsCount;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentException("Building must contain at least 1 level.");
            }

            _levelsCount = value;
        }
    }
}