using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Moq;
using ResultMonad;
using Services;
using Services.Map;
using UnitTests.Fixtures;
using UseCases.RetrieveBuildingByAddress;

namespace UnitTests.UseCases;

public class RetrieveBuildingByAddressUseCaseTests
{
    private readonly AddressTestFixture _addressFixture;

    public RetrieveBuildingByAddressUseCaseTests()
    {
        _addressFixture = new AddressTestFixture();
    }

    [Fact]
    public async Task Run_ShouldReturnCorrectValues()
    {
        // Arrange
        var addressParserMock = new Mock<IAddressParser>();
        var mapProviderServiceMock = new Mock<IMapProviderService>();

        var buildingName = "Здание 2";
        var address = _addressFixture.AddressFaker.Generate();
        var addressDto = _addressFixture.CreateAddressDto(address);
        var buildingInfo = new BuildingBasementInformation
        {
            Address = address,
            Geometry = new Polygon(new List<LineString>
            {
                new(new List<Position>
                {
                    new(1, 2),
                    new(1, 3),
                    new(2, 3),
                    new(1, 2)
                })
            }),
            LevelsCount = 17
        };

        _addressFixture.SetupAddressParsing(addressParserMock, address);
        mapProviderServiceMock
            .Setup(x => x.GetBuildingInformationAsync(It.Is<Address>(a => a == address), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<BuildingBasementInformation, ErrorMessage>(buildingInfo));

        var args = new RetrieveBuildingByAddressArgs { Address = addressDto };
        var useCase = new RetrieveBuildingByAddressUseCase(mapProviderServiceMock.Object, addressParserMock.Object);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var value = result.Value;
        Assert.Equal(address.ToString(), value.Address);
        Assert.Equal(buildingInfo.LevelsCount, value.LevelsCount);
        Helpers.AssertGeometryEquality(buildingInfo.Geometry.Coordinates.Unpack(), value.Geometry.Coordinates);

        addressParserMock.Verify(x => x.TryParseStreet(addressDto.Street, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        addressParserMock.Verify(x => x.TryParseHouse(addressDto.House, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        mapProviderServiceMock.Verify(x => x.GetBuildingInformationAsync(It.Is<Address>(a => a == address), It.IsAny<CancellationToken>()), Times.Once());
    }
}