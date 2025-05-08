using System.Text.RegularExpressions;

namespace WebApi.Utils;

internal static partial class PropertyNameConverter
{
    public static string SnakeCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        // "aB" -> "a_B"
        var result = RegularTransitions().Replace(propertyName, "$1_$2");

        // "HTMLParser" -> "HTML_Parser"
        result = AcronymTransitions().Replace(result, "$1_$2");

        return result.ToLower();
    }

    [GeneratedRegex("([A-Z]+)([A-Z][a-z])")]
    private static partial Regex AcronymTransitions();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex RegularTransitions();
}