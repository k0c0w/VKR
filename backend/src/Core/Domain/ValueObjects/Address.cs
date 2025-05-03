namespace Domain;

public sealed record Address
{
    public string City { get; } = "";

    /// <summary>
    /// For example, "Кремлёвская" in "улица Кремлёвская, 35к1".
    /// </summary>
    public string StreetName { get; } = "";

    /// <summary>
    /// For example, "улица" in "улица Кремлёвская, 35к1".
    /// </summary>
    public string StreetType { get; } = "";

    /// <summary>
    /// For example, "35" in "улица Кремлёвская, 35к1".
    /// </summary>
    public string HouseNumber { get; } = "";

    /// <summary>
    /// For example, "к1" in "улица Кремлёвская, 35к1".
    /// Might be empty.
    /// </summary>
    public string HouseUnit { get; } = "";

    public Address(string city, string streetName, string streetType, string houseNumber, string? houseUnit = null)
    {
        City = city;
        StreetType = streetType;
        StreetName = streetName;
        HouseNumber = houseNumber;
        HouseUnit = houseUnit ?? "";
    }

    public override string ToString()
    {
        return $"г. {City}, {StreetType} {StreetName}, {HouseNumber}{HouseUnit}";
    }
}