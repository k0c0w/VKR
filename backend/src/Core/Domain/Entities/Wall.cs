using Domain.Aggregates;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Entities;

public sealed class Wall : IHaveIdentity<Guid>
{
    public Guid Id { get; }

    internal Level BelongsToLevel { get; private set; }
    
    public Guid BelongsToLevelId => BelongsToLevel.Id;
    
    public LineString Geometry { get; private set; }

    internal Wall(Level levelContainingWall, LineString wallGeometry) 
        : this(Guid.CreateVersion7(), levelContainingWall, wallGeometry)
    {
    }

    private Wall(Guid id, Level levelContainingWall, LineString wallGeometry)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Empty guid.", nameof(id));
        }
        ArgumentNullException.ThrowIfNull(levelContainingWall, nameof(levelContainingWall));
        ArgumentNullException.ThrowIfNull(wallGeometry, nameof(wallGeometry));
        ArgumentNullException.ThrowIfNull(levelContainingWall, nameof(levelContainingWall));

        Id = id;
        BelongsToLevel = levelContainingWall;
        Geometry = wallGeometry;
        BelongsToLevel = levelContainingWall;
    }

    public static void CreateExistingRoomAtLevel(Level level, Guid wallId,  LineString geometry)
    {
        var wall = new Wall(wallId, level, geometry);
        level.AddStructure(wall);
    }
}
