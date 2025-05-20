using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Building : IHaveIdentity<Guid>
{
    private readonly Dictionary<int, Level> _levels;

    public Guid Id { get; }

    public Address Address { get; }

    public Polygon BasementGeometry { get; }

    public int LevelsCount => _levels.Count;

    public IReadOnlyCollection<Level> Levels => _levels.Values;
    
    public Building(Address address, Polygon buildingBasement)
    {
        Id = Guid.CreateVersion7();
        BasementGeometry = buildingBasement;
        Address = address;
        _levels = new Dictionary<int, Level>
        {
            [1] = new (Id, 1)
        };
    }
    
    private Building(Guid id, Address address, Polygon buildingBasement, IEnumerable<Level> buildingLevels)
    {
        id.ThrowIfEmpty(nameof(id));
        Id = id;
        Address = address;
        BasementGeometry = buildingBasement;
        _levels = buildingLevels.ToDictionary(key => key.Number, value => value);
    }

    public ResultWithError<ErrorMessage> CreateLevel(int number, string levelName)
    {
        if (_levels.ContainsKey(number))
        {
            return ResultWithError.Fail( ErrorMessage.ValidationError($"Этаж с номером {number} уже существует."));
        }

        var level = new Level(Id, number, levelName);
        _levels.Add(number, level);
        
        return ResultWithError.Ok<ErrorMessage>();
    }

    public void RemoveLevel(Level level)
    {
        _levels.Remove(level.Number);
    }
    
    public Result<Level, ErrorMessage> GetLevel(int number)
        => _levels.TryGetValue(number, out var level) 
            ? Result.Fail<Level, ErrorMessage>(ErrorMessage.EntityNotfoundError) 
            : Result.Ok<Level, ErrorMessage>(level!);
    
    public static Building CreateExistingBuildingInstance(Guid id, 
        Address address, 
        Polygon buildingBasement, 
        IEnumerable<Level> buildingLevels)
    {
        return new Building(id, address, buildingBasement, buildingLevels);
    }
}