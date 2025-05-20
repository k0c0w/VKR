using Domain.ValueObjects;
using UnitTests.Fixtures;

namespace UnitTests.ValueObjects;

public class AddressTests
{
    [Theory]
    [InlineData("г. К, улица Кремлёвская, 35", "К", "Кремлёвская", "улица", "35", null)]
    [InlineData("г. М, улица Кремлёвская, 35 а", "М", "Кремлёвская", "улица", "35", "а")]
    public void ToString_ShouldReturnAddressString(string expectedAddressString, string city, string streetName, string streetType, string houseNumber, string? houseUnit)
    {
        var address = new Address(city, streetName, streetType, houseNumber, houseUnit);

        var actualString = address.ToString();
        
        Assert.Equal(expectedAddressString, actualString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FromString_ShouldReturnAddress_WhenCorrectInput(bool withHouseUnit)
    {
        var fakeAddress = new AddressTestFixture(withHouseUnit).AddressFaker.Generate();
        var addressString = fakeAddress.ToString();

        var parsedAddress = Address.FromString(addressString);

        Assert.Equal(fakeAddress, parsedAddress);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("г. К")]
    [InlineData("К")]
    [InlineData("г.")]
    [InlineData("г. К, 2")]
    [InlineData("г., ул к, 2")]
    [InlineData("г. К, ул к, ")]
    public void FromString_ShouldThrowArgumentException_WhenIncorrectInput(string input)
    {
        Assert.Throws<ArgumentException>(() => Address.FromString(input));
    }
    
    [Fact]
    public void GetStreet_ShouldReturnCorrectStreet()
    {
        var address = new Address("Казань", "Крем", "улица", "34");

        var street = address.GetStreet();
        
        Assert.NotNull(street);
        Assert.NotEmpty(street);
        Assert.Equal("улица Крем", street);
    }
    
    [Theory]
    [InlineData("34 а", "34", "а")]
    [InlineData( "34", "34", null)]
    public void GetHouse_ShouldReturnCorrectStreet(string expectedHouse, string house, string? unit)
    {
        var address = new Address("Казань", "Крем", "улица", house, unit);

        var result = address.GetHouse();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(expectedHouse, result);
    }
}