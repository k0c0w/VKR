using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Moq;
using ResultMonad;
using UnitTests.Fixtures;
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
        var buildingInfo1 = (Guid.CreateVersion7(), address1, "Учебное здание 1");
        var buildingInfo2 = (Guid.CreateVersion7(), address2, "Учебное здание 2");
        var buildingInfos = new[] { buildingInfo1, buildingInfo2 };

        buildingRepositoryMock
            .Setup(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok< (Guid, Address, string)[], ErrorMessage>(buildingInfos));

        var useCase = new GetAvailablePlansListUseCase(buildingRepositoryMock.Object);

        // Act
        var result = await useCase.RunAsync(CancellationToken.None);
        var planShortcuts = result.Value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, planShortcuts.Length);
        Assert.Equal(buildingInfo1.Item1.ToString(), planShortcuts[0].BuildingId);
        Assert.Equal(buildingInfo1.Item2.ToString(), planShortcuts[0].BuildingAddress);
        Assert.Equal(buildingInfo1.Item3, planShortcuts[0].BuildingName);
        Assert.Equal(buildingInfo2.Item1.ToString(), planShortcuts[1].BuildingId);
        Assert.Equal(buildingInfo2.Item2.ToString(), planShortcuts[1].BuildingAddress);
        Assert.Equal(buildingInfo2.Item3, planShortcuts[1].BuildingName);

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
            .ReturnsAsync(Result.Fail<(Guid, Address, string)[], ErrorMessage>(expectedError));

        var useCase = new GetAvailablePlansListUseCase(buildingRepositoryMock.Object);

        // Act
        var result = await useCase.RunAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Error);

        buildingRepositoryMock.Verify(repo => repo.GetAllBuildingInformationAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}