namespace Domain.ValueObjects;

public sealed record Location 
{
    /// <summary>
    /// Region of address.
    /// For example, "Респ.Марий Эл, р-нВолжский", "Респ.Арабская, г.Каир", "Респ.Татарстан, г.Елабуга"
    /// </summary>
    public string Region { get; }
    
    /// <summary>
    /// Address of object. Might be empty.
    /// Examples of nonempty addresses "ст-цаЗеленчукская, ул.Ленина, 41", "г.Казань, ул.Проспект Победы, 32"
    /// </summary>
    public string Address { get; }
    
    public Location(string region, string? address = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(region);
        Address = address ?? string.Empty;
        Region = region;
    }
}