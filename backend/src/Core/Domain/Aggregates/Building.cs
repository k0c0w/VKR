using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Aggregates;

public class Building : IHaveIdentity<Guid>
{
    private readonly Dictionary<uint, Level> _levels;

    public Guid Id { get; }

    public Location Location { get; private set; }

    public Polygon BasementGeometry { get; private set; }

    public string Name { get; private set; }
    
    public IReadOnlyCollection<Level> Levels => _levels.Values;
    
    internal Building(Location location, string buildingName, Polygon buildingBasement)
    {
        Id = Guid.CreateVersion7();
        BasementGeometry = buildingBasement;
        Location = location;
        ArgumentException.ThrowIfNullOrEmpty(buildingName);
        Name = buildingName;

        _levels = [];
    }
    
    private Building(Guid id, Location location, string buildingName, Polygon buildingBasement, IEnumerable<Level> buildingLevels)
    {
        id.ThrowIfEmpty(nameof(id));
        Id = id;
        Location = location;
        Name = buildingName;
        BasementGeometry = buildingBasement;
        _levels = buildingLevels.ToDictionary(key => key.Id.Number, value => value);
    }

    public Result<Level, ErrorMessage> CreateLevel(string levelName)
    {
        var level = new Level(Id,(uint)_levels.Count + 1, levelName);
        _levels.Add(level.Id.Number, level);
        
        return Result.Ok<Level, ErrorMessage>(level);
    }

    public Result<Level, ErrorMessage> GetLevel(uint number)
    {
        return _levels.TryGetValue(number, out var level) 
            ? Result.Ok<Level, ErrorMessage>(level) 
            : Result.Fail<Level, ErrorMessage>(ErrorMessage.DomainError($"Этаж с номером {number} не найден."));
    }

    public async Task<ResultWithError<ErrorMessage>> ChangeNameAsync(string newName, IBuildingRepository buildingRepository, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(newName))
        {
            return ResultWithError.Fail( ErrorMessage.ValidationError("Имя не может быть пустым."));
        }

        var updateName = await buildingRepository.UpdateAsync(this, ct);
        if (updateName.IsSuccess)
        {
            Name = newName;
        }

        return updateName;
    }

    public async Task<ResultWithError<ErrorMessage>> UpdateBasementGeometry(Polygon newBasementGeometry, IBuildingRepository buildingRepository, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(newBasementGeometry);

        var update = await buildingRepository.UpdateAsync(this, ct);

        if (update.IsFailure && update.Error == ErrorMessage.EntityNotfoundError)
        {
            return ResultWithError.Fail(ErrorMessage.DataInconsistency);
        }
        if (update.IsSuccess)
        {
            BasementGeometry = newBasementGeometry;
        }

        return update;
    }
    
    public static Building CreateExistingBuildingInstance(Guid id, 
        Location location, 
        string buildingName,
        Polygon buildingBasement, 
        IEnumerable<Level> buildingLevels)
    {
        return new Building(id, location, buildingName, buildingBasement, buildingLevels);
    }
}