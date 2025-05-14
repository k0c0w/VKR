namespace Common.Dto;

public readonly record struct AddressDto(string City, string Street, string House)
{
    public void Deconstruct(out string city, out string street, out string house)
    {
        city = City;
        street = Street;
        house = House;
    }
}
