using Domain.ValueObjects;

namespace UnitTests.ValueObjects;

public class AddressTests
{
    [Theory]
    [InlineData("Кремлёвская", "улица", "35", null)]
    [InlineData("Кремлёвская", "улица", "35", "a")]
    public void ToString_ShouldReturnAddressString(string streetName, string streetType, string houseNumber, string? houseUnit)
    {
        const string city = "Казань";
        var address = new Address(city, streetName, streetType, houseNumber, houseUnit);
        var expectedAddressString = $"г. {city}, {streetType} {streetName}, {houseNumber}{houseUnit??string.Empty}";

        var actualString = address.ToString();
        
        Assert.Equal(expectedAddressString, actualString);
    }
}