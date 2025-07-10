using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using ResultMonad;

namespace DataAccess.Repositories;

internal class RoomRepository(NpgsqlDataSource dataSource, ILogger<IRoomRepository> logger) 
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IRoomRepository
{
    private const string TableName = "buildings_rooms";
    
    public async Task<ResultWithError<ErrorMessage>> UpdateRoomDescriptionAsync(long roomId, RoomDescription roomDescription, CancellationToken ct)
    {
        const string updateSql = $"""
                                      UPDATE {TableName} room
                                         SET   geometry = @{nameof(roomDescription.Geometry)}
                                             , type = @{nameof(roomDescription.Type)}
                                             , name = @{nameof(roomDescription.Name)}
                                       WHERE room.id = @{nameof(roomId)};
                                  """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value;
        await using var command = new NpgsqlCommand(updateSql, conn);
        command.Parameters.AddWithValue(nameof(roomDescription.Geometry), NpgsqlDbType.Jsonb, Serialize(roomDescription.Geometry));
        command.Parameters.AddWithValue(nameof(roomDescription.Type), (short)roomDescription.Type);
        command.Parameters.AddWithValue(nameof(roomDescription.Name),  roomDescription.Name ?? string.Empty);
        command.Parameters.AddWithValue(nameof(roomId), roomId);

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
            
            return ResultWithError.Fail(ErrorMessage.DomainError($"Произошла ошибка при обновлении описания помещения с id {roomId}."));
        }
    }

    public async Task<ResultWithError<ErrorMessage>> AddAsync(Room room, CancellationToken ct)
    {
        const string insertSqlRoom = $"""
           INSERT INTO buildings_rooms (id, building_id, level_number, type, geometry, name)
           VALUES (@{nameof(room.Id)},@{nameof(room.BelongsToLevel.Id.BuildingId)},@{nameof(room.BelongsToLevel.Id.Number)},@{nameof(room.Type)},@{nameof(room.Geometry)},@{nameof(room.Name)});
        """;

        var openConnResult = await GetOpenedConnectionAsync(ct);
        if (openConnResult.IsFailure)
        {
            return ResultWithError.Fail(ErrorMessage.DomainError("Не удалось получить ответ от базы данных."));
        }

        await using var conn = openConnResult.Value;
        await using var command = new NpgsqlCommand(insertSqlRoom, conn);
        command.Parameters.AddWithValue(nameof(room.Id), room.Id);
        command.Parameters.AddWithValue(nameof(room.BelongsToLevel.Id.BuildingId), room.BelongsToLevel.Id.BuildingId);
        command.Parameters.AddWithValue(nameof(room.BelongsToLevel.Id.Number), NpgsqlDbType.Integer, (int)room.BelongsToLevel.Id.Number);
        command.Parameters.AddWithValue(nameof(room.Type), (short)room.Type);
        command.Parameters.AddWithValue(nameof(room.Geometry), NpgsqlDbType.Jsonb, room.Geometry);
        command.Parameters.AddWithValue(nameof(room.Name), room.Name);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(ct);

            return affectedRows > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.DomainError($"Не удалось создать комнату с id {room.Id}."));
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            
            return ResultWithError.Fail(ErrorMessage.DomainError($"Не удалось создать комнату с id {room.Id}."));
        }
    }
    
    public async Task<ResultWithError<ErrorMessage>> RemoveAsync(Room room, CancellationToken ct)
    {
        const string deleteSql = $"""
            DELETE FROM {TableName} room
             WHERE room.id = @{nameof(room.Id)};
         """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value; 
        await using var command = new NpgsqlCommand(deleteSql, conn);
        command.Parameters.AddWithValue(nameof(room.Id), room.Id);
        
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
            
            return ResultWithError.Fail(ErrorMessage.DomainError($"Произошла ошибка при удалении помещения с id {room.Id}."));
        }
    }
}