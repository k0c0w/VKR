namespace Services;

public interface IAddressParser
{
    public bool TryParseStreet(string street, out string streetType, out string streetName);

    public bool TryParseHouse(string house, out string houseNumber, out string unitNumber);
}