using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public sealed class ItEquipment : IHaveIdentity<string>
{
    internal Room? BelongsToRoom { get; private set; }
    
    public string Id => Description.Id;

    public string Name => Description.Name;
    
    public ItEquipmentGeometry? Geometry { get; private set; }
    
    public ItEquipmentDescription Description { get; }

    public ItEquipment(ItEquipmentDescription description, ItEquipmentGeometry? geometry = default)
    {
        Geometry = geometry;
        
        ArgumentNullException.ThrowIfNull(description, nameof(description));
        Description = description;
    }

    internal void AttachToRoom(Room room)
    {
        if (room.Id != Description.LocationAudienceCatalogueId)
        {
            throw new InvalidOperationException("Action might lead to data inconsistence",
                new ArgumentException($"{nameof(room)}.{nameof(Room.Id)} != {nameof(Description)}.{nameof(Description.LocationAudienceCatalogueId)}"));
        }
        
        BelongsToRoom = room;

        if (Geometry is null || !BelongsToRoom.IsPositionInside(Geometry.Coordinates))
        {
            Geometry = new ItEquipmentGeometry(BelongsToRoom.GetCenter().Coordinates);
        }
    }
    
    public async Task<ResultWithError<ErrorMessage>> MoveToPositionAsync(IPosition position, 
        IItEquipmentRepository equipmentRepository, 
        CancellationToken ct = default)
    {
        if (BelongsToRoom is not null && !BelongsToRoom.IsPositionInside(position))
        {
            return ResultWithError.Fail(
                ErrorMessage.DomainError(
                    "Не возможно переместить оборудование в комнату, к которой оно не принадлежит."));
        }

        var geometry = new ItEquipmentGeometry(position);

        var oldGeometry = Geometry;
        Geometry = geometry;
        
        var update = await equipmentRepository.UpdateAsync(this, ct);
        if (update.IsFailure)
        {
            Geometry = oldGeometry;
        }

        return update;
    }
    
    public void SetDescription(ItEquipmentDescription description)
    {
        if (Description is not null)
        {
            throw new InvalidOperationException($"{nameof(Description)} is already set.");
        }
        if (Id != description.Id)
        {
            throw new ArgumentException(
                $"{nameof(Id)} and {nameof(ItEquipmentDescription)}.{nameof(ItEquipmentDescription.Id)} did not match.");
        }
    }
}