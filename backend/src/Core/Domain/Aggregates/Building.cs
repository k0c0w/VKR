using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace Domain.Aggregates;

public class Building : IHaveIdentity<Guid>
{
    private readonly List<Level> _levels;

    public Guid Id { get; }

    public Address Address => BuildingInformation.Address;

    public Polygon BasementGeometry => BuildingInformation.Geometry;

    public uint LevelsCount => BuildingInformation.LevelsCount;

    public IReadOnlyCollection<Level> Levels => _levels;
    
    private BuildingInformation BuildingInformation { get; set; }

    public Building(BuildingInformation buildingInformation)
    {
        Id = Guid.CreateVersion7();
        BuildingInformation = buildingInformation;

        _levels = Enumerable.Range(0, (int)BuildingInformation.LevelsCount)
            .Select((i) => new Level(Id, i + 1))
            .ToList();
    }
    
    public Building(BuildingInformation buildingInformation, IEnumerable<Level> buildingLevels) 
        : this(Guid.CreateVersion7(), buildingInformation, buildingLevels)
    {

    }

    private Building(Guid id, BuildingInformation buildingInformation, IEnumerable<Level> buildingLevels)
    {
        Id = id;
        BuildingInformation = buildingInformation;
        _levels = buildingLevels.ToList();

        if (buildingInformation.LevelsCount != _levels.Count)
        {
            throw new ArgumentException(
                $"The {nameof(BuildingInformation.LevelsCount)} and count of actual {nameof(buildingLevels)} did not match.", 
                nameof(buildingInformation));
        }
    }

    public static Building CreateExistingBuildingInstance(Guid id, BuildingInformation buildingInformation, IEnumerable<Level> buildingLevels)
    {
        return new Building(id, buildingInformation, buildingLevels);
    }
}