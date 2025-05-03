using Domain;
using Domain.Errors;
using Domain.GeoJson;
using Moq;
using ResultMonad;
using Services.Implementation.OSM;
using Services.Map;
using UseCases.RetrieveBuildingByAddress;

namespace UnitTests.Domain.UseCases;

public class RetrieveBuildingByAddressUseCaseTests
{
    [Fact]
    public async Task Run_ShouldReturnCorrectValues()
    {
        const string expectedCity = "Казань",
            expectedStreetType = "улица",
            expectedStreetName = "Кремлёвская",
            expectedHouse = "35",
            expectedStreet = $"{expectedStreetType} {expectedStreetName}";
        var expectedAddress = new Address(expectedCity, expectedStreet, expectedStreetType, expectedHouse);
        var expectedBuildingInfo = new BuildingInformation
        {
            Address = expectedAddress,
            LevelsCount = 17,
            Geometry = new BuildingGeometry([
                [
                    new LatLng { Lat = 1, Lng = 2 },
                    new LatLng { Lat = 1, Lng = 3 },
                    new LatLng { Lat = 1, Lng = 4 },
                    new LatLng { Lat = 1, Lng = 2 },
                ]
            ])
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
            .Setup(x => x.ParseStreet(expectedStreet))
            .Returns(() => (expectedStreetType, expectedStreetName));
        addressParserMock.Setup(x => x.ParseHouse(expectedHouse))
            .Returns(() => (expectedHouse, ""));

        var args = new RetrieveBuildingByAddressDto
        {
            City = expectedCity,
            Street = expectedStreet,
            HouseNumber = expectedHouse
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