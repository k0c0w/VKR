using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public sealed class ItEquipment : IHaveIdentity<string>
{
    public string Id => Description.Id;

    public ItEquipmentGeometry? Geometry { get; private set; }
    
    public ItEquipmentDescription Description { get; }

    public string InstallationLevelName => Description.Location.LevelName;

    public ItEquipment(ItEquipmentDescription description, ItEquipmentGeometry? geometry = default)
    {
        Geometry = geometry;
        
        ArgumentNullException.ThrowIfNull(description, nameof(description));
        Description = description;
    }

    public void MoveToPosition(ItEquipmentGeometry geometry)
    {
        Geometry = geometry;
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