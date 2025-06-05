using System.Text;
using Common;

namespace Services.Implementation.Utils;

internal static class CustomHttpUtility
{
    public static string UrlEncode(string value)
    {
        var windows1251 = Encoding.GetEncoding("windows-1251");
        var bytes = windows1251.GetBytes(value);

        using var encoded = new ValueStringBuilder();
        foreach (var b in bytes)
        {
            encoded.Append('%');
            encoded.Append(b.ToString("X2"));
        }

        return encoded.ToString();
    }
}