namespace Services.Implementation.OSM;

public interface IAddressParser
{
    public (string StreetType, string StreetName) ParseStreet(string street);

    public (string HouseNumber, string UnitNumber) ParseHouse(string houseNumber);
}