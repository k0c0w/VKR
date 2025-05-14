using OutParsing;

namespace Domain.ValueObjects;

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
        return string.IsNullOrEmpty(HouseUnit) ?
            $"г. {City}, {StreetType} {StreetName}, {HouseNumber}"
            : $"г. {City}, {StreetType} {StreetName}, {HouseNumber} {HouseUnit}";
    }

    public static Address FromString(string addressToStringResult)
    {
        OutParser.Parse(addressToStringResult, "г. {city}, {streetType} {streetName}, {houseNumber} {houseUnit}",
            out string city, out string streetType,
            out string streetName, out string houseNumber,
            out string? houseUnit);
        
        return new Address(city, streetName, streetType, houseNumber, houseUnit);
    }
}