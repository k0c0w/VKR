using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using ResultMonad;

namespace DataAccess.Repositories;

public class WallRepository(NpgsqlDataSource dataSource, ILogger<IWallRepository> logger) 
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IWallRepository
{
    private const string TableName = "buildings_walls";
    
    public async Task<ResultWithError<ErrorMessage>> UpdateGeometryAsync(Guid id, LineString geometry, CancellationToken ct)
    {
        const string updateSql = $"""
                                      UPDATE {TableName} wall
                                         SET geometry = @{nameof(geometry)}
                                       WHERE wall.id = @{nameof(id)};
                                  """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value;
        await using var command = new NpgsqlCommand(updateSql, conn);
        command.Parameters.AddWithValue(nameof(geometry), NpgsqlDbType.Jsonb, Serialize(geometry));
        command.Parameters.AddWithValue(nameof(id), id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(ct);

            return affectedRows > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return ResultWithError.Fail(ErrorMessage.DomainError($"Произошла ошибка при обновлении геометрии стены с uuid {id}."));
        }
    }

    public async Task<ResultWithError<ErrorMessage>> RemoveAsync(Wall wall, CancellationToken ct)
    {
        const string deleteSql = $"""
                                      DELETE FROM {TableName}
                                       WHERE id = @{nameof(wall.Id)};
                                  """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value;
        await using var command = new NpgsqlCommand(deleteSql, conn);
        command.Parameters.AddWithValue(nameof(wall.Id), wall.Id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(ct);

            return affectedRows > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return ResultWithError.Fail(ErrorMessage.DomainError($"Произошла ошибка при удалении стены с uuid {wall.Id}."));
        }
    }

    public async Task<ResultWithError<ErrorMessage>> AddAsync(Wall wall, CancellationToken ct)
    {
        const string insertSql= $"""
              INSERT INTO buildings_walls (id, building_id, level_number, geometry)
              VALUES (@{nameof(Wall.Id)},@{nameof(Wall.BelongsToLevel.Id.BuildingId)},@{nameof(Wall.BelongsToLevel.Id.Number)},@{nameof(Wall.Geometry)});
        """;
        var openConnectionResult = await GetOpenedConnectionAsync(ct);
        if (openConnectionResult.IsFailure)
        {
            return ResultWithError.Fail(openConnectionResult.Error);
        }

        await using var conn = openConnectionResult.Value;
        await using var command = new NpgsqlCommand(insertSql, conn);
        command.Parameters.AddWithValue(nameof(Wall.Id), wall.Id);
        command.Parameters.AddWithValue(nameof(Wall.BelongsToLevel.Id.BuildingId), wall.BelongsToLevel.Id.BuildingId);
        command.Parameters.AddWithValue(nameof(Wall.BelongsToLevel.Id.Number), NpgsqlDbType.Integer, (int)wall.BelongsToLevel.Id.Number);
        command.Parameters.AddWithValue(nameof(Wall.Geometry), NpgsqlDbType.Jsonb, Serialize(wall.Geometry));
        
        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(ct);

            return affectedRows > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(
                    ErrorMessage.ValidationError("Не удалось сохранить стену."));
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return ResultWithError.Fail(ErrorMessage.DomainError("Не удалось сохранить стену."));
        }
    }
}