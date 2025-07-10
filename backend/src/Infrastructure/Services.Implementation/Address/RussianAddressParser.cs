using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using AddressDto = Services.Address.Address;

namespace Services.Implementation.Address;

public partial class RussianAddressParser : Services.Address.IAddressParser
{
    private static readonly FrozenDictionary<string, string> StreetTypeShortcutMap = new KeyValuePair<string, string>[]
    {
        new("ул.", "улица"),
        new("бул.", "бульвар"),
        new("б-р", "бульвар"),
        new("дор.", "дорога"),
        new("маг.", "магистраль"),
        new("наб.", "набережная"),
        new("пер.", "переулок"),
        new("пл.", "площадь"),
        new("пр.", "проспект"),
        new("пр-д.", "проезд"),
        new("просп.", "проспект"),
        new("пр-кт", "проспект"),
        new("туп.", "тупик"),
        new("ш.", "шоссе")
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, string> SettlementShortcutTypeMap =
        new KeyValuePair<string, string>[]
        {
            new("г.", "город"),
            new("пгт.", "поселок городского типа"),
            new("рп.", "рабочий поселок"),
            new("кп.", "курортный поселок"),
            new("гп.", "городской поселок"),
            new("п.", "поселок"),
            new("аал.", "аал"),
            new("арбан.", "арбан"),
            new("аул.", "аул"),
            new("в-ки.", "выселки"),
            new("г-к.", "городок"),
            new("з-ка.", "заимка"),
            new("п-к.", "починок"),
            new("киш.", "кишлак"),
            new("п. ст.", "поселок при станции"),
            new("п. ж/д ст.", "поселок при железнодорожной станции"),
            new("ж/д бл-ст.", "железнодорожный блокпост"),
            new("ж/д б-ка.", "железнодорожная будка"),
            new("ж/д в-ка.", "железнодорожная ветка"),
            new("ж/д к-ма.", "железнодорожная казарма"),
            new("ж/д к-т.", "железнодорожный комбинат"),
            new("ж/д пл-ма.", "железнодорожная платформа"),
            new("ж/д пл-ка.", "железнодорожная площадка"),
            new("ж/д п.п.", "железнодорожный путевой пост"),
            new("ж/д о.п.", "железнодорожный остановочный пункт"),
            new("ж/д рзд.", "железнодорожный разъезд"),
            new("ж/д ст.", "железнодорожная станция"),
            new("м-ко.", "местечко"),
            new("д.", "деревня"),
            new("с.", "село"),
            new("сл.", "слобода"),
            new("ст.", "станция"),
            new("ст-ца.", "станица"),
            new("у.", "улус"),
            new("х.", "хутор"),
            new("рзд.", "разъезд"),
            new("зим.", "зимовье")
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> StreetTypeVariations =
        StreetTypeShortcutMap.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> SettlementTypeVariations =
        SettlementShortcutTypeMap.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool TryParseAddress(string addressString, [NotNullWhen(true)] out AddressDto? address)
    {
        address = default;
        if (string.IsNullOrWhiteSpace(addressString))
        {
            return false;
        }

        var tokens = addressString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length is < 2 or > 3)
        {
            return false;
        }

        var settlement = tokens[0];
        if (!TryParseObjectTypeAndName(settlement, SettlementShortcutTypeMap, SettlementTypeVariations,
                out var settlementType, out var settlementNameSpan))
        {
            return false;
        }

        var street = tokens[1];

        if (!TryParseObjectTypeAndName(street, StreetTypeShortcutMap, StreetTypeVariations,
                out var streetType, out var streetName))
        {
            return false;
        }


        var house = tokens.Length == 3 ? tokens[2] : string.Empty;
        if (!TryParseHouse(house, IsComplexStreet(ref streetName), out var houseNumber, out var houseUnit))
        {
            return false;
        }

        MutateComplexStreet(ref streetName);
        if (streetName.IsEmpty)
        {
            return false;
        }

        address = new AddressDto(settlementType,
            settlementNameSpan.ToString(),
            streetType,
            streetName.ToString(),
            houseNumber,
            houseUnit);

        return true;
    }

    private static bool TryParseHouse(string house, bool isComplexStreet, out string houseNumber, out string houseUnit)
    {
        houseNumber = houseUnit = string.Empty;

        if (string.IsNullOrEmpty(house))
        {
            return false;
        }

        var houseSpan = house.AsSpan();
        if (isComplexStreet)
        {
            var lastSlashIndex = houseSpan.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {
                houseSpan = houseSpan[..lastSlashIndex];
            }
        }

        var regexInput = house.Length == houseSpan.Length ? house : houseSpan.ToString();
        var match = HouseParsingRegex().Match(regexInput);
        if (!match.Success)
        {
            return false;
        }

        houseNumber = match.Groups[1].Value;
        houseUnit = match.Groups[2].Value.Trim();
        return true;
    }

    private static bool IsComplexStreet(ref ReadOnlySpan<char> streetName) => streetName.IndexOf('/') != -1;

    private static void MutateComplexStreet(ref ReadOnlySpan<char> streetName)
    {
        var slashIndex = streetName.IndexOf('/');
        if (slashIndex == -1)
        {
            return;
        }

        streetName = streetName[..slashIndex];
    }

    private static bool TryParseObjectTypeAndName(
        string typeNameObject,
        IDictionary<string, string> shortcutTypesMap,
        ISet<string> allPossibleTypeVariations,
        out string completeObjectType,
        out ReadOnlySpan<char> objectName)
    {
        completeObjectType = string.Empty;
        objectName = ReadOnlySpan<char>.Empty;
        if (string.IsNullOrWhiteSpace(typeNameObject))
        {
            return false;
        }

        var typeVariation = allPossibleTypeVariations.FirstOrDefault(typeNameObject.StartsWith);
        if (typeVariation is null)
        {
            return false;
        }

        completeObjectType = shortcutTypesMap.TryGetValue(typeVariation, out var completeTypeVariation)
            ? completeTypeVariation // prefix is shortcut
            : typeVariation; // prefix is complete type

        if (typeVariation.Length + 1 >= typeNameObject.Length)
        {
            return false;
        }

        objectName = typeNameObject.AsSpan()
            .Slice(typeVariation.Length, typeNameObject.Length - typeVariation.Length)
            .Trim();

        return !string.IsNullOrEmpty(completeObjectType) && !objectName.IsEmpty;
    }

    [GeneratedRegex(@"^(\d+(?:/\d+)?)(.*)", RegexOptions.Compiled)]
    private static partial Regex HouseParsingRegex();
}