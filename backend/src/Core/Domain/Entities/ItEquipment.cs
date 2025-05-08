namespace Domain.Entities;

public class ItEquipment : IHaveIdentity<string>
{
    public string Id { get; protected init; }
    
    public string Name { get; protected set; }

    public ItEquipment(string inventoryNumber)
    {
        ArgumentException.ThrowIfNullOrEmpty(inventoryNumber?.Trim());
        Id = inventoryNumber;
    }
}