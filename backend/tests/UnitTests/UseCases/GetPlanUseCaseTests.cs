using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Moq;
using ResultMonad;
using Services;
using UnitTests.UseCases.Fixtures;
using UseCases.Plans;

namespace UnitTests.UseCases;

public sealed class GetPlanUseCaseTests
{
    private readonly AddressTestFixture _addressFixture;

    public GetPlanUseCaseTests()
    {
        _addressFixture = new AddressTestFixture();
    }

    [Fact]
    public async Task Run_ShouldReturnBuildingPlan_WhenAllSucceeds()
    {
        // Arrange
        var addressParserMock = new Mock<IAddressParser>();
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var address = _addressFixture.AddressFaker.Generate();
        var addressDto = _addressFixture.CreateAddressDto(address);
        var buildingId = Guid.NewGuid();
        var basementGeometry = new Polygon(new List<LineString>
        {
            new(new List<Position>
            {
                new(1, 2),
                new(3, 4),
                new(5, 6),
                new(1, 2)
            })
        });
        var levelId = new LevelIdentity(buildingId, 1);
        var wall = Wall.CreateExistingWallInstance(Guid.NewGuid(), levelId, new LineString(new List<Position> { new(0, 0), new(1, 1) }));
        var roomDescription = new RoomDescription
        {
            Geometry = new Polygon(new List<LineString>
            {
                new(new List<Position> { new(0, 0), new(1, 0), new(1, 1), new(0, 0) })
            }),
            Type = RoomType.Audience,
            ArchitectualId = "R1",
            Name = "Room1"
        };
        var room = Room.CreateExistingButEmptyRoom(Guid.NewGuid(), levelId, roomDescription);
        var level = Level.CreateExistingLevel(buildingId, 1, "First", new List<Room> { room }, new List<Wall> { wall });
        var buildingInfo = new BuildingInformation
        {
            Address = address,
            Geometry = basementGeometry,
            LevelsCount = 1
        };
        var building = Building.CreateExistingBuildingInstance(buildingId, buildingInfo, new List<Level> { level });

        _addressFixture.SetupAddressParsing(addressParserMock, address);
        buildingRepositoryMock
            .Setup(x => x.GetBuildingAsync(It.Is<IBuildingRepository.BuildingFilter>(f => f.Address == address), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Building, ErrorMessage>(building));

        var useCase = new GetPlanUseCase(addressParserMock.Object, buildingRepositoryMock.Object);
        var args = new GetPlanUseCaseArgs(addressDto);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);
        var plan = result.Value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(buildingId, plan.BuildingId);
        Assert.Equal(basementGeometry, plan.BasementGeometry);
        Assert.Single(plan.Levels);
        var planLevel = plan.Levels.First();
        Assert.Equal(1, planLevel.Number);
        Assert.Equal("First", planLevel.Name);
        Assert.Equal(2, planLevel.Structure.Count());
        Assert.Empty(planLevel.ItEquipments);

        addressParserMock.Verify(x => x.TryParseStreet(addressDto.Street, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        addressParserMock.Verify(x => x.TryParseHouse(addressDto.House, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        buildingRepositoryMock.Verify(x => x.GetBuildingAsync(It.Is<IBuildingRepository.BuildingFilter>(f => f.Address == address), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Run_ShouldReturnError_WhenStreetParsingFails()
    {
        // Arrange
        var addressParserMock = new Mock<IAddressParser>();
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var address = _addressFixture.AddressFaker.Generate();
        var addressDto = _addressFixture.CreateAddressDto(address);

        _addressFixture.SetupAddressParsing(addressParserMock, address, streetSuccess: false);

        var useCase = new GetPlanUseCase(addressParserMock.Object, buildingRepositoryMock.Object);
        var args = new GetPlanUseCaseArgs(addressDto);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(new ErrorMessage("Не удалось распарсить улицу."), result.Error);

        addressParserMock.Verify(x => x.TryParseStreet(addressDto.Street, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        addressParserMock.Verify(x => x.TryParseHouse(It.IsAny<string>(), out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Never());
        buildingRepositoryMock.Verify(x => x.GetBuildingAsync(It.IsAny<IBuildingRepository.BuildingFilter>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Run_ShouldReturnError_WhenHouseParsingFails()
    {
        // Arrange
        var addressParserMock = new Mock<IAddressParser>();
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var address = _addressFixture.AddressFaker.Generate();
        var addressDto = _addressFixture.CreateAddressDto(address);

        _addressFixture.SetupAddressParsing(addressParserMock, address, streetSuccess: true, houseSuccess: false);

        var useCase = new GetPlanUseCase(addressParserMock.Object, buildingRepositoryMock.Object);
        var args = new GetPlanUseCaseArgs(addressDto);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(new ErrorMessage("Не удалось распарсить дом."), result.Error);

        addressParserMock.Verify(x => x.TryParseStreet(addressDto.Street, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        addressParserMock.Verify(x => x.TryParseHouse(addressDto.House, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        buildingRepositoryMock.Verify(x => x.GetBuildingAsync(It.IsAny<IBuildingRepository.BuildingFilter>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Run_ShouldReturnError_WhenBuildingNotFound()
    {
        // Arrange
        var addressParserMock = new Mock<IAddressParser>();
        var buildingRepositoryMock = new Mock<IBuildingRepository>();
        
        var address = _addressFixture.AddressFaker.Generate();
        var addressDto = _addressFixture.CreateAddressDto(address);

        _addressFixture.SetupAddressParsing(addressParserMock, address);
        buildingRepositoryMock
            .Setup(x => x.GetBuildingAsync(It.Is<IBuildingRepository.BuildingFilter>(f => f.Address == address), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Building, ErrorMessage>(ErrorMessage.EntityNotfoundError));

        var useCase = new GetPlanUseCase(addressParserMock.Object, buildingRepositoryMock.Object);
        var args = new GetPlanUseCaseArgs(addressDto);

        // Act
        var result = await useCase.RunAsync(args, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorMessage.EntityNotfoundError, result.Error);

        addressParserMock.Verify(x => x.TryParseStreet(addressDto.Street, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        addressParserMock.Verify(x => x.TryParseHouse(addressDto.House, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once());
        buildingRepositoryMock.Verify(x => x.GetBuildingAsync(It.Is<IBuildingRepository.BuildingFilter>(f => f.Address == address), It.IsAny<CancellationToken>()), Times.Once());
    }
}