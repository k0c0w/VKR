using Domain;
using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Moq;
using ResultMonad;
using Services;
using Services.Map;
using UseCases.RetrieveBuildingByAddress;

namespace UnitTests.UseCases;

public class RetrieveBuildingByAddressUseCaseTests
{
    [Fact]
    public async Task Run_ShouldReturnCorrectValues()
    {
        const string expectedCity = "Казань";
        string expectedStreetType = "улица",
            expectedStreetName = "Кремлёвская";
        var expectedHouse = "35";
        var expectedUnitNumber = string.Empty;
        var expectedStreet = $"{expectedStreetType} {expectedStreetName}";
        
        var expectedAddress = new Address(expectedCity, expectedStreet, expectedStreetType, expectedHouse);
        var expectedBuildingInfo = new BuildingInformation
        {
            Address = expectedAddress,
            LevelsCount = 17,
            Geometry = new Polygon([new LineString(
                [
                    new Position(latitude: 1, longitude: 2),
                    new Position(latitude: 1, longitude: 3),
                    new Position(latitude: 1, longitude: 4),
                    new Position(latitude: 1, longitude: 2),
                ]
            )])
        };
        var expectedBuildingInfoResult = Result.Ok<BuildingInformation, ErrorMessage>(expectedBuildingInfo);

        var serviceMock = new Mock<IMapProviderService>();
        serviceMock.Setup(x => x.GetBuildingInformationAsync(
                It.IsAny<Address>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBuildingInfoResult)
            .Verifiable();

        var addressParserMock = new Mock<IAddressParser>();
        addressParserMock
            .Setup(x => x.TryParseStreet(expectedStreet, out expectedStreetType, out expectedStreetName))
            .Returns(true);
        addressParserMock.Setup(x => x.TryParseHouse(expectedHouse, out expectedHouse, out expectedUnitNumber))
            .Returns(true);

        var args = new RetrieveBuildingByAddressDto
        {
            City = expectedCity,
            Street = expectedStreet,
            House = expectedHouse
        };
        var useCase = new RetrieveBuildingByAddressUseCase(serviceMock.Object, addressParserMock.Object);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);
        var value = result.Value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAddress.ToString(), value.Address);
        Assert.Equal(expectedBuildingInfo.LevelsCount, value.LevelsCount);
        Helpers.AssertGeometryEquality(expectedBuildingInfo.Geometry.Coordinates, value.Geometry);
    }
}