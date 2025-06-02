using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Building : IHaveIdentity<Guid>
{
    private readonly Dictionary<string, Level> _levels;

    public Guid Id { get; }

    public Address Address { get; }

    public Polygon BasementGeometry { get; }

    public string Name { get; }
    
    public int LevelsCount => _levels.Count;

    public IReadOnlyCollection<Level> Levels => _levels.Values;
    
    public Building(Address address, string buildingName, Polygon buildingBasement)
    {
        Id = Guid.CreateVersion7();
        BasementGeometry = buildingBasement;
        Address = address;
        ArgumentException.ThrowIfNullOrEmpty(Name);
        Name = buildingName;
        
        _levels = new Dictionary<string, Level>()
        {
            [Level.FirstLevelDefaultName] = new(Id, Level.FirstLevelDefaultName),
        };
    }
    
    private Building(Guid id, Address address, string buildingName, Polygon buildingBasement, IEnumerable<Level> buildingLevels)
    {
        id.ThrowIfEmpty(nameof(id));
        Id = id;
        Address = address;
        Name = buildingName;
        BasementGeometry = buildingBasement;
        _levels = buildingLevels.ToDictionary(key => key.Name, value => value);
    }

    public ResultWithError<ErrorMessage> CreateLevel(string levelName)
    {
        if (_levels.ContainsKey(levelName))
        {
            return ResultWithError.Fail( ErrorMessage.ValidationError($"{levelName} уже существует."));
        }

        var level = new Level(Id, levelName);
        _levels.Add(levelName, level);
        
        return ResultWithError.Ok<ErrorMessage>();
    }

    public void RemoveLevel(Level level)
    {
        _levels.Remove(level.Name);
    }
    
    public Result<Level, ErrorMessage> GetLevel(string levelName)
        => _levels.TryGetValue(levelName, out var level) 
            ? Result.Ok<Level, ErrorMessage>(level!)
            : Result.Fail<Level, ErrorMessage>(ErrorMessage.EntityNotfoundError); 
    
    public static Building CreateExistingBuildingInstance(Guid id, 
        Address address, 
        string buildingName,
        Polygon buildingBasement, 
        IEnumerable<Level> buildingLevels)
    {
        return new Building(id, address, buildingName, buildingBasement, buildingLevels);
    }
}