using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Moq;
using ResultMonad;
using UnitTests.UseCases.Fixtures;
using UseCases.Plans;

namespace UnitTests.UseCases;

public sealed class GetAvailablePlansListUseCaseTests
{
    private readonly AddressTestFixture _addressFixture;

    public GetAvailablePlansListUseCaseTests()
    {
        _addressFixture = new AddressTestFixture();
    }

    [Fact]
    public async Task RunAsync_ReturnsBuildingPlanShortcuts_WhenRepositorySucceeds()
    {
        // Arrange
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var address1 = _addressFixture.AddressFaker.Generate();
        var address2 = _addressFixture.AddressFaker.Generate();
        var buildingInfo1 = new BuildingInformation
        {
            Address = address1,
            Geometry = new Polygon(new List<LineString>
            {
                new(new List<Position>
                {
                    new(55.75, 49.11),
                    new(55.75, 49.12),
                    new(55.76, 49.12),
                    new(55.76, 49.11),
                    new(55.75, 49.11)
                })
            }),
            LevelsCount = 3
        };
        var buildingInfo2 = new BuildingInformation
        {
            Address = address2,
            Geometry = new Polygon(new List<LineString>
            {
                new(new List<Position>
                {
                    new(55.76, 37.61),
                    new(55.76, 37.62),
                    new(55.77, 37.62),
                    new(55.77, 37.61),
                    new(55.76, 37.61)
                })
            }),
            LevelsCount = 5
        };
        var buildingInfos = new[] { buildingInfo1, buildingInfo2 };

        buildingRepositoryMock
            .Setup(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<BuildingInformation[], ErrorMessage>(buildingInfos));

        var useCase = new GetAvailablePlansListUseCase(buildingRepositoryMock.Object);

        // Act
        var result = await useCase.RunAsync(CancellationToken.None);
        var planShortcuts = result.Value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, planShortcuts.Length);
        Assert.Equal(buildingInfo1.Address.ToString(), planShortcuts[0].BuildingAddress);
        Assert.Equal(buildingInfo2.Address.ToString(), planShortcuts[1].BuildingAddress);

        buildingRepositoryMock.Verify(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsError_WhenRepositoryFails()
    {
        // Arrange
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var expectedError = ErrorMessage.RepositorySpecificErrors.GetError;

        buildingRepositoryMock
            .Setup(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<BuildingInformation[], ErrorMessage>(expectedError));

        var useCase = new GetAvailablePlansListUseCase(buildingRepositoryMock.Object);

        // Act
        var result = await useCase.RunAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Error);

        buildingRepositoryMock.Verify(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}