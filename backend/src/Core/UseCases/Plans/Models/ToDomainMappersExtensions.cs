using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace UseCases.Plans.Models;

internal static class ToDomainMappersExtensions
{
    public static ItEquipment ToDomain(this BuildingPlanItEquipment equipment, ItEquipmentDescription description)
    {
        if (description.Id != equipment.InventoryNumber)
        {
            throw new ArgumentException($"{nameof(ItEquipmentDescription)}.{nameof(ItEquipmentDescription.Id)} and {nameof(equipment.InventoryNumber)} must be same.");
        }
        
        var coords = equipment.LocationPoint.Coordinates;
        var location = new ItEquipmentGeometry(new Position(longitude: coords[0], latitude: coords[1]));
        
        return new ItEquipment(description, location);
    }
}