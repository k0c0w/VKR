using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Entities;

public sealed class Wall : IHaveIdentity<Guid>
{
    public Guid Id { get; }

    public LevelIdentity BelongsToLevel { get; private set; }
    
    public LineString Geometry { get; private set; }

    internal Wall(LevelIdentity levelContainingWall, LineString wallGeometry) 
        : this(Guid.CreateVersion7(), levelContainingWall, wallGeometry)
    {
    }

    private Wall(Guid id, LevelIdentity levelContainingWall, LineString wallGeometry)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Empty guid.", nameof(id));
        }
        ArgumentNullException.ThrowIfNull(levelContainingWall, nameof(levelContainingWall));
        ArgumentNullException.ThrowIfNull(wallGeometry, nameof(wallGeometry));

        Id = id;
        BelongsToLevel = levelContainingWall;
        Geometry = wallGeometry;
    }

    public static Wall CreateExistingWallInstance(Guid wallId, LevelIdentity belongingLevelId, LineString geometry)
        => new Wall(wallId, belongingLevelId, geometry);
}
