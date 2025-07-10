using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Entities;

public sealed class Wall : IHaveIdentity<Guid>
{
    public Guid Id { get; }

    public Level? BelongsToLevel { get; private set; }
    
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

    public async Task<ResultWithError<ErrorMessage>> UpdateGeometryAsync(LineString newGeometry, IWallRepository wallRepository,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(newGeometry);

        var updateResult = await wallRepository.UpdateGeometryAsync(Id, newGeometry, ct);
        if (updateResult.IsFailure && updateResult.Error == ErrorMessage.EntityNotfoundError)
        {
            return ResultWithError.Fail(ErrorMessage.DataInconsistency);
        }
        if (updateResult.IsSuccess)
        {
            Geometry = newGeometry;
        }

        return updateResult;
    }

    internal void DetachFromLevel()
    {
        BelongsToLevel = null;
    }
    
    public static void CreateExistingRoomAtLevel(Level level, Guid wallId,  LineString geometry)
    {
        var wall = new Wall(wallId, level, geometry);
        level.AddStructure(wall);
    }
}
