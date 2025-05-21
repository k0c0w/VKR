using System.Buffers;

namespace Domain.ValueObjects;

public sealed record Address : IEquatable<Address>
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

    public string GetStreet() => $"{StreetType} {StreetName}";

    public string GetHouse() => string.IsNullOrEmpty(HouseUnit) ? HouseNumber : $"{HouseNumber} {HouseUnit}";

    private const string CityPrefix = "г. ";
    public override string ToString()
    {
        return string.IsNullOrEmpty(HouseUnit)
            ? $"{CityPrefix}{City}, {StreetType} {StreetName}, {HouseNumber}"
            : $"{CityPrefix}{City}, {StreetType} {StreetName}, {HouseNumber} {HouseUnit}";
    }

    public static Address FromString(string addressToStringResult)
    {
        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries;
        var tokens = addressToStringResult.Replace('ё', 'е').Split(", ", splitOptions);
        if (tokens.Length != 3)
        {
            ThrowArgumentException();
        }

        var assumedCity = tokens[0];
        if (!assumedCity.StartsWith(CityPrefix))
        {
            ThrowArgumentException();
        }
        assumedCity = assumedCity.Replace(CityPrefix, string.Empty);

        var sep = ArrayPool<char>.Shared.Rent(1);
        sep[0] = ' ';
        try
        {
            var assumedStreet = tokens[1];
            var streetTokens = assumedStreet.Split(sep, 2, splitOptions);
            if (streetTokens.Length != 2)
            {
                ThrowArgumentException();
            }

            var assumedHouse = tokens[2];
            var houseTokens = assumedHouse.Split(sep, splitOptions);
            if (houseTokens.Length != 1 && houseTokens.Length != 2)
            {
                ThrowArgumentException();
            }
        
            return new Address(assumedCity, streetTokens[1], streetTokens[0], houseTokens[0], houseTokens.Length > 1 ? houseTokens[1] : default);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(sep);
        }
    }

    private static void ThrowArgumentException()
    {
        throw new ArgumentException($"Provided string was not the result of {nameof(ToString)} method.");
    }
}