using Domain.Aggregates;
using Domain.Repositories;
using IntegrationTests.DatabaseTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Domain.Entities;
using Domain.Errors;

namespace IntegrationTests.DatabaseTests;

[Collection(nameof(DatabaseTestsCollection))]
public class IBuildingRepositoryTests : DbTestsBase
{
    [Fact]
    public async Task AddAndGetBuilding_ShouldRetrieveCorrectBuilding()
    {
        // Arrange
        var repository = ServiceProvider.GetRequiredService<IBuildingRepository>();
        var building = BuildingFixture.CreateTestBuilding();
        var idFilter = IBuildingRepository.BuildingFilter.IdFilter(building.Id);
        var addressFilter = IBuildingRepository.BuildingFilter.AddressFilter(building.Address);

        // Act
        var addResult = await repository.AddAsync(building, CancellationToken.None);
        Assert.True(addResult.IsSuccess, "Adding building failed.");

        var foundByIdResult = await repository.GetBuildingAsync(idFilter, CancellationToken.None);
        Assert.True(foundByIdResult.IsSuccess, "Retrieving building by ID failed.");
        var foundById = foundByIdResult.Value;

        var foundByAddressResult = await repository.GetBuildingAsync(addressFilter, CancellationToken.None);
        Assert.True(foundByAddressResult.IsSuccess, "Retrieving building by address failed.");
        var foundByAddress = foundByAddressResult.Value;

        // Assert
        AssertBuilding(building, foundById);
        AssertBuilding(building, foundByAddress);
    }

    [Fact]
    public async Task AddMultipleBuildingsAndGetAllInformation_ShouldRetrieveAll()
    {
        // Arrange
        var repository = ServiceProvider.GetRequiredService<IBuildingRepository>();
        var building1 = BuildingFixture.CreateTestBuilding();
        var building2 = BuildingFixture.CreateTestBuilding();

        // Act
        var addResult1 = await repository.AddAsync(building1, CancellationToken.None);
        Assert.True(addResult1.IsSuccess, "Adding first building failed.");

        var addResult2 = await repository.AddAsync(building2, CancellationToken.None);
        Assert.True(addResult2.IsSuccess, "Adding second building failed.");

        var allInfosResult = await repository.GetAllBuildingInformationAsync(CancellationToken.None);
        Assert.True(allInfosResult.IsSuccess, "Retrieving all building information failed.");
        var allInfos = allInfosResult.Value;

        // Assert
        Assert.True(allInfos.Length >= 2);
        var info1 = allInfos.FirstOrDefault(i => i.Address == building1.Address);
        Assert.NotNull(info1);
        Assert.Equal(building1.Address, info1.Address);
        Assert.Equal(building1.BasementGeometry, info1.Geometry);
        Assert.Equal(building1.LevelsCount, info1.LevelsCount);

        var info2 = allInfos.FirstOrDefault(i => i.Address == building2.Address);
        Assert.NotNull(info2);
        Assert.Equal(building2.Address, info2.Address);
        Assert.Equal(building2.BasementGeometry, info2.Geometry);
        Assert.Equal(building2.LevelsCount, info2.LevelsCount);
    }

    [Fact]
    public async Task GetNonExistentBuilding_ShouldReturnFailure()
    {
        // Arrange
        var repository = ServiceProvider.GetRequiredService<IBuildingRepository>();
        var nonExistentId = Guid.NewGuid();
        var filter = IBuildingRepository.BuildingFilter.IdFilter(nonExistentId);

        // Act
        var result = await repository.GetBuildingAsync(filter, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorMessage.EntityNotfoundError, result.Error);
    }

    private static void AssertBuilding(Building expected, Building actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Address, actual.Address);
        Assert.Equal(expected.BasementGeometry, actual.BasementGeometry);
        Assert.Equal(expected.LevelsCount, actual.LevelsCount);

        // Assert levels equality
        Assert.Equal(expected.Levels.Count, actual.Levels.Count);
        var expectedLevels = expected.Levels.OrderBy(l => l.Number).ToList();
        var actualLevels = actual.Levels.OrderBy(l => l.Number).ToList();

        for (var i = 0; i < expectedLevels.Count; i++)
        {
            AssertLevel(expectedLevels[i], actualLevels[i]);
        }
    }

    private static void AssertLevel(Level expected, Level actual)
    {
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Name, actual.Name);

        // Assert rooms
        Assert.Equal(expected.Rooms.Count, actual.Rooms.Count);
        var expectedRooms = expected.Rooms.OrderBy(r => r.Id).ToList();
        var actualRooms = actual.Rooms.OrderBy(r => r.Id).ToList();

        for (var i = 0; i < expectedRooms.Count; i++)
        {
            AssertRoom(expectedRooms[i], actualRooms[i]);
        }

        // Assert walls
        Assert.Equal(expected.Walls.Count, actual.Walls.Count);
        var expectedWalls = expected.Walls.OrderBy(w => w.Id).ToList();
        var actualWalls = actual.Walls.OrderBy(w => w.Id).ToList();

        for (int i = 0; i < expectedWalls.Count; i++)
        {
            AssertWall(expectedWalls[i], actualWalls[i]);
        }
    }

    private static void AssertRoom(Room expected, Room actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Name ?? "", actual.Name ?? "");
        Assert.Equal(expected.Geometry, actual.Geometry);
        Assert.Equal(expected.ArchitectualId, actual.ArchitectualId);
        Assert.Equal(expected.BelongsToLevel.BuildingId, actual.BelongsToLevel.BuildingId);
        Assert.Equal(expected.BelongsToLevel.LevelNumber, actual.BelongsToLevel.LevelNumber);
    }

    private static void AssertWall(Wall expected, Wall actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Geometry, actual.Geometry);
        Assert.Equal(expected.BelongsToLevel.BuildingId, actual.BelongsToLevel.BuildingId);
        Assert.Equal(expected.BelongsToLevel.LevelNumber, actual.BelongsToLevel.LevelNumber);
    }
}