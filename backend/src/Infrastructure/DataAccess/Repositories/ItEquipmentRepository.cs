using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using ResultMonad;
using Services.EKsu;

namespace DataAccess.Repositories;

internal class ItEquipmentRepository(
    NpgsqlDataSource dataSource, 
    IItEquipmentCatalogue catalogue,
    ILogger<ItEquipmentRepository> logger)
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IItEquipmentRepository
{
    private const string TableName = "buildings_it_equipment";
    
    public async Task<Result<ItEquipment[], ErrorMessage>> GetItEquipmentByRoomIdsAsync(long[] roomIds, CancellationToken ct)
    {
        var getEquipmentInfoTask = catalogue.GetAllItEquipmentByRoomIdsAsync(roomIds, ct);
        var getEquipmentGeometryTask = GetItEquipmentGeometryByRoomIdsAsync(roomIds, ct);

        await Task.WhenAll(getEquipmentGeometryTask, getEquipmentInfoTask);

        var getEquipmentInfoResult = getEquipmentInfoTask.GetAwaiter().GetResult();
        var getEquipmentGeometryResult = getEquipmentGeometryTask.GetAwaiter().GetResult();

        if (getEquipmentInfoResult.IsFailure)
        {
            return Result.Fail<ItEquipment[], ErrorMessage>(getEquipmentInfoResult.Error);
        }

        if (getEquipmentGeometryResult.IsFailure)
        {
            return Result.Fail<ItEquipment[], ErrorMessage>(getEquipmentGeometryResult.Error);
        }

        var equipmentInfo = getEquipmentInfoResult.Value!;
        var equipmentGeometry = getEquipmentGeometryResult.Value!;

        var result = new List<ItEquipment>();
        foreach (var info in equipmentInfo)
        {
            var geometry = equipmentGeometry.TryGetValue(info.Id, out var value) ? value : default;

            var equipment = new ItEquipment(info, geometry);
            
            result.Add(equipment);
        }
        
        return Result.Ok<ItEquipment[], ErrorMessage>(result.ToArray());
    }
    
    public async Task<ResultWithError<ErrorMessage>> UpdateAsync(ItEquipment equipment, CancellationToken ct)
    {
        const string updateGeometrySql = $"""
            INSERT INTO {TableName} (inventory_number, room_id, geometry)
            VALUES (@{nameof(equipment.Id)}, @{nameof(equipment.Description.LocationAudienceCatalogueId)}, @{nameof(equipment.Geometry)})
            ON CONFLICT (inventory_number)
            DO UPDATE SET geometry = @{nameof(equipment.Geometry)};
        """;
        
        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }
        
        await using var conn = openConResult.Value;
        try
        {
            await using var itEquipmentGeometry = new NpgsqlCommand(updateGeometrySql, conn);
            itEquipmentGeometry.Parameters.AddWithValue(nameof(equipment.Geometry), NpgsqlDbType.Jsonb, Serialize(equipment.Geometry));
            itEquipmentGeometry.Parameters.AddWithValue(nameof(equipment.Id), equipment.Id);
            itEquipmentGeometry.Parameters.AddWithValue(nameof(equipment.Description.LocationAudienceCatalogueId), equipment.Description.LocationAudienceCatalogueId);
            
            var affectedRowsCount = await itEquipmentGeometry.ExecuteNonQueryAsync(ct);

            return affectedRowsCount > 0 
                ? ResultWithError.Ok<ErrorMessage>() 
                : ResultWithError.Fail(ErrorMessage.DomainError(ErrorMessage.EntityNotfoundError)); 
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return ResultWithError.Fail(ErrorMessage.SystemError($"Произошла ошибка при сохранении местоположения ИТ-оборудования c id {equipment.Id}."));
        }
    }

    private async Task<Result<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>> GetItEquipmentGeometryByRoomIdsAsync(
        long[] roomIds,
        CancellationToken ct)
    {
        if (roomIds.Length == 0)
        {
            return Result.Ok<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>(
                new Dictionary<string, ItEquipmentGeometry>());
        }
        var roomParams = roomIds.Select((id, i) => new NpgsqlParameter($"room{i}", id)).ToArray();
        var sql = $"""
            SELECT
                      bie.inventory_number as id
                    , bie.geometry as geometry
              FROM buildings_it_equipment bie
             WHERE bie.room_id in ({string.Join(',', roomParams.Select(x => $"@{x.ParameterName}"))});
        """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>(openConResult.Error);
        }
        
        await using var conn = openConResult.Value!;
        var result = new Dictionary<string, ItEquipmentGeometry>();
        try
        {
            await using var itEquipmentGeometry = new NpgsqlCommand(sql, conn);
            itEquipmentGeometry.Parameters.AddRange(roomParams);
            
            var reader = await itEquipmentGeometry.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var id = reader.GetString(0);
                var geometry = reader.GetFieldValue<ItEquipmentGeometry>(1);

                result.Add(id, geometry);
            }

            return Result.Ok<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>(result);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return Result.Fail<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>(ErrorMessage.AbstractError);
        }
    }
}