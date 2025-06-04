using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Npgsql;
using ResultMonad;
using Services.EKsu;

namespace DataAccess.Repositories;

internal class ItEquipmentRepository(
    NpgsqlDataSource dataSource, 
    IItEquipmentCatalogue catalogue,
    ILogger<ItEquipmentRepository> logger)
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IItEquipmentRepository
{
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

    private async Task<Result<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>> GetItEquipmentGeometryByRoomIdsAsync(
        long[] roomIds,
        CancellationToken ct)
    {
        var roomParams = roomIds.Select((_, i) => $"room{i}").ToArray();
        var sql = $"""
            SELECT
                      bie.inventory_number as id
                    , bie.geometry as geometry
                    , b.geometry as building_basement_geometry
              FROM buildings_it_equipment bie
              JOIN buildings_levels bl ON bl.level_id = bl.id
              JOIN buildings b ON b.id = bl.building_id
             WHERE b.address = ({string.Join(',', roomParams)});
        """;
        var sqlParams = roomIds.Select((x, i) => new NpgsqlParameter(roomParams[i], x))
            .ToArray();

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
            itEquipmentGeometry.Parameters.AddRange(sqlParams);
            
            var reader = await itEquipmentGeometry.ExecuteReaderAsync(ct);

            Polygon? buildingBasementPolygon = default;
            while (await reader.ReadAsync(ct))
            {
                var id = reader.GetString(0);
                var geometry = reader.GetFieldValue<ItEquipmentGeometry>(1);

                if (buildingBasementPolygon == default)
                {
                    buildingBasementPolygon = reader.GetFieldValue<Polygon>(2);
                }

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