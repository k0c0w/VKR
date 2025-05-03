using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Services.Implementation.OSM;

public partial class AddressParserImpl : IAddressParser
{
    private static readonly FrozenDictionary<string, string> ShortcutMap = new KeyValuePair<string, string>[]
        {
            new ("ул.", "улица"),
            new ("бул.", "бульвар"),
            new ("б-р", "бульвар"),
            new ("дор.", "дорога"),
            new ("маг.", "магистраль"),
            new ("наб.", "набережная"),
            new ("пер.", "переулок"),
            new ("пл.", "площадь"),
            new ("пр.", "проспект"),
            new ("пр-д.", "проезд"),
            new ("просп.", "проспект"),
            new ("пр-кт", "проспект"),
            new ("туп.", "тупик"),
            new ("ш.", "шоссе")
        }
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    
    private static readonly FrozenSet<string> StreetTypes = new []
    {
        // Full words
        "улица", "шоссе", "тракт", "аллея", "вал", "взвоз", "въезд", "дорога", "заезд", "кольцо", 
        "линия", "магистраль", "набережная", "переулок", "площадь", "проспект", "проезд", "проулок", 
        "разъезд", "спуск", "съезд", "территория", "тупик",
        // Abbreviations
        "бул.", "б-р", "дор.", "ул.", "маг.", "наб.", "пер.", "пл.", "пр.", "пр-д.", "просп.", "пр-кт", "туп.", "ш."
    }
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    
    public bool TryParseStreet(string street, out string streetType, out string streetName)
    {
        if (!string.IsNullOrEmpty(street))
        {
            var words = street.Split(' ',StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                if (StreetTypes.Contains(words[0]) && words.Length >= 2)
                {
                    streetType = ShortcutMap.TryGetValue(words[0], out var longVersion) ? longVersion : words[0];
                    streetName = string.Join(' ', words.Skip(1));
            
                    return true;
                }
                
                if (StreetTypes.Contains(words[^1]))
                {
                    streetType = ShortcutMap.TryGetValue(words[^1], out var longVersion) ? longVersion : words[^1];
                    streetName = string.Join(" ", words.Take(words.Length - 1));
                    return true;
                }
            }
        }

        streetType = streetName = string.Empty;

        return false;
    }

    public bool TryParseHouse(string house, out string houseNumber, out string unitNumber)
    {
        houseNumber = unitNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(house))
        {
            return false;
        }

        var parts = house.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        var housePart = parts[0];
        
        var match = HouseNumberWithSuffixRegex().Match(housePart);
        if (match.Success)
        {
            var numberPart = match.Groups[1].Value;
            if (SingleNumberRegex().IsMatch(numberPart) || SlashedNumberRegex().IsMatch(numberPart))
            {
                houseNumber = numberPart;
                unitNumber = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                if (parts.Length > 1)
                {
                    unitNumber = parts[1];
                }

                return true;
            }
        }

        if (SingleNumberRegex().IsMatch(housePart) || SlashedNumberRegex().IsMatch(housePart))
        {
            houseNumber = housePart;
            if (parts.Length > 1)
            {
                unitNumber = parts[1];
            }
            return true;
        }

        return false;
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex SingleNumberRegex();
    
    [GeneratedRegex(@"^\d+/\d+$")]
    private static partial Regex SlashedNumberRegex();
    
    [GeneratedRegex(@"^(\d+\/?\d*)([а-яА-Я]\d*)*$")]
    private static partial Regex HouseNumberWithSuffixRegex();
}