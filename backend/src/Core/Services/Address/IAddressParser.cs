using System.Diagnostics.CodeAnalysis;

namespace Services.Address;

public interface IAddressParser
{
    public bool TryParseAddress(string addressString, [NotNullWhen(true)] out Address? address);
}