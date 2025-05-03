using Services.Implementation.OSM;

namespace UnitTests.Domain.Services;

public class AddressParserImplTests
{
    private readonly AddressParserImpl _parser = new();
    
    [Theory]
    [InlineData("улица Кремлёвская", "улица", "Кремлёвская")]
    [InlineData("Кремлёвская улица", "улица", "Кремлёвская")]
    public void TryParseStreet_ReturnsTrueCorrectly(string street, string expectedType, string expectedName)
    {
        // Act
        var result = _parser.TryParseStreet(street, out var streetType, out var streetName);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedType, streetType);
        Assert.Equal(expectedName, streetName);
    }

    [Theory]
    [InlineData("ул. Пушкина", "улица", "Пушкина")]
    [InlineData("Пушкина ул.", "улица", "Пушкина")]
    [InlineData("ш. Ленинградское", "шоссе", "Ленинградское")]
    [InlineData("2-ое Юго-Западное шоссе", "шоссе", "2-ое Юго-Западное")]
    public void TryParseStreet_ReturnsTrue_WhenAbbreviationUsed(string input, string expectedType, string expectedName)
    {
        // Act
        var result = _parser.TryParseStreet(input, out string streetType, out string streetName);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedType, streetType);
        Assert.Equal(expectedName, streetName);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Кремлёвская")]
    [InlineData("Кремлёвская Пушкина")]
    public void TryParseStreet_ReturnsFalseCorrectly(string wrongStreet)
    {
        // Act
        var result = _parser.TryParseStreet(wrongStreet, out _, out _);

        // Assert
        Assert.False(result);
    }

    // TryParseHouse Tests
    [Theory]
    [InlineData("35", true,"35", "")]
    [InlineData("35/8", true, "35/8", "")]
    [InlineData("35а", true, "35", "а")]
    [InlineData("35к1", true,"35", "к1")]
    [InlineData("35 к1", true,"35", "к1")]
    [InlineData("35/18 А", true, "35/18", "А")]
    
    [InlineData(null, false, "", "")]
    [InlineData("", false, "", "")]
    [InlineData("abc", false, "", "")]
    [InlineData("35/abc", false, "", "")]
    [InlineData("35.05", false, "", "")]
    public void TryParseHouse_ReturnsTrue(string house, bool expectedResult, string expectedHouseNumber, string expectedUnit)
    {
        // Act
        var result = _parser.TryParseHouse(house, out var houseNumber, out var unitNumber);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(expectedHouseNumber, houseNumber);
            Assert.Equal(expectedUnit, unitNumber);
        }
    }
}