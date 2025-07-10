using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Room : IHaveIdentity<long>
{
    private readonly Dictionary<string, ItEquipment> _itEquipments;

    private Point? _centerOfRoom;

    public long Id { get; }
    
    public Level? BelongsToLevel { get; private set; }

    public LevelIdentity? BelongsToLevelId => BelongsToLevel?.Id;

    public RoomType Type => RoomDescription.Type;

    public string? Name => RoomDescription.Name;

    public Polygon Geometry => RoomDescription.Geometry;

    public IReadOnlyCollection<ItEquipment> ItEquipments => _itEquipments.Values;
    
    private RoomDescription RoomDescription { get; set; }
    
    internal Room (long id, Level belongsToLevel, RoomDescription roomDescription)
    {
        if (id == 0)
        {
            throw new ArgumentException("Default id value met.", nameof(id));
        }
        ArgumentNullException.ThrowIfNull(roomDescription, nameof(roomDescription));

        Id = id;
        RoomDescription = roomDescription;
        BelongsToLevel = belongsToLevel;
        _itEquipments = new Dictionary<string, ItEquipment>();
    }
    
    public ResultWithError<ErrorMessage> AddEquipment(ItEquipment equipment)
    {
        if (equipment.Description.LocationAudienceCatalogueId != Id)
        {
            return ResultWithError.Fail<ErrorMessage>(ErrorMessage.DomainError("ИТ-оборудование не принадлежит помещению."));
        }
        if (_itEquipments.TryAdd(equipment.Id, equipment))
        {
            equipment.AttachToRoom(this);
            
            return ResultWithError.Ok<ErrorMessage>();
        }

        return ResultWithError.Fail(ErrorMessage.EntityAlreadyExists);
    }

    public async Task<ResultWithError<ErrorMessage>> ChangeTypeAsync(RoomType type, IRoomRepository repository,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(type))
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError($"Не известный тип помещения: {type}."));
        }

        var updatedDescription = RoomDescription with { Type = type };
        var updateResult = await repository.UpdateRoomDescriptionAsync(Id, updatedDescription, ct);
        if (updateResult.IsSuccess)
        {
            RoomDescription = updatedDescription;
        }

        return updateResult;
    }
    
    public async Task<ResultWithError<ErrorMessage>> UpdateGeometryAsync(Polygon newGeometry,
        IRoomRepository roomRepository,
        IItEquipmentRepository itEquipmentRepository,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(newGeometry);
        
        var newDescription = RoomDescription with { Geometry = newGeometry };
        var updateRoomGeometry = await roomRepository.UpdateRoomDescriptionAsync(Id, newDescription, ct);
        if (updateRoomGeometry.IsFailure)
        {
            return updateRoomGeometry.Error == ErrorMessage.EntityNotfoundError 
                ? ResultWithError.Fail(ErrorMessage.DataInconsistency) 
                : updateRoomGeometry;
        }

        RoomDescription = newDescription;
        var center = GetCenter();
        foreach (var equipment in _itEquipments.Values
                     .Where(equipment => !IsPositionInside(equipment.Geometry.Coordinates)))
        {
            await equipment.MoveToPositionAsync(center.Coordinates, itEquipmentRepository, ct);
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    }
    
    internal Point GetCenter()
    {
        if (_centerOfRoom is not null)
        {
            return _centerOfRoom;
        }
        
        var exteriorRing = Geometry.Coordinates[0];

        var coordinates = exteriorRing.Coordinates;

        var xSum = 0d;
        var ySum = 0d;
        var vertexCount = coordinates.Count;

        foreach (var position in coordinates)
        {
            xSum += position.Longitude;
            ySum += position.Latitude; 
        }

        var centroidX = xSum / vertexCount;
        var centroidY = ySum / vertexCount;

        var center = new Point(new Position(latitude: centroidY, longitude: centroidX));

        _centerOfRoom ??= center;

        return center;
    }

    internal bool IsPositionInside(IPosition position)
    {
        var exteriorRing = Geometry.Coordinates[0];
        var coords = exteriorRing.Coordinates;
        var n = coords.Count - 1;
        var inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var currentVertex = coords[i];
            var previousVertex = coords[j];

            if (((currentVertex.Latitude > position.Latitude) != (previousVertex.Latitude > position.Latitude)) &&
                (position.Longitude < (previousVertex.Longitude - currentVertex.Longitude) * (position.Latitude - currentVertex.Latitude) / 
                    (previousVertex.Latitude - currentVertex.Latitude) + currentVertex.Longitude))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    internal void DetachFromLevel()
    {
        BelongsToLevel = null;
    }
    
    public static void CreateExistingRoomAtLevel(Level roomLevel, long roomId, RoomDescription description, IEnumerable<ItEquipment> itEquipments)
    {
        var room = new Room(roomId, roomLevel, description);
        foreach (var equipment in itEquipments)
        {
            room.AddEquipment(equipment);
        }
        
        roomLevel.AddStructure(room);
    }
}