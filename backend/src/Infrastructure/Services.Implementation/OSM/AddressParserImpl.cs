namespace Services.Implementation.OSM;

public class AddressParserImpl : IAddressParser
{
    public (string StreetType, string StreetName) ParseStreet(string street)
    {
        return (StreetType: "улица", StreetName: "Кремлевская");
    }

    public (string HouseNumber, string UnitNumber) ParseHouse(string houseNumber)
    {
        return (HouseNumber: "35", UnitNumber:"");
    }
}