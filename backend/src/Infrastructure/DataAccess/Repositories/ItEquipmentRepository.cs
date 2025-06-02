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

namespace DataAccess.Repositories;

internal class ItEquipmentRepository(
    NpgsqlDataSource dataSource, 
    IItEquipmentCatalogue catalogue,
    ILogger<ItEquipmentRepository> logger)
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IItEquipmentRepository
{
    public async Task<Result<ItEquipment[], ErrorMessage>> GetItEquipmentByAddressAsync(Address address, CancellationToken ct)
    {
        var getEquipmentInfoTask = catalogue.GetAllItEquipmentAtBuildingAsync(address, ct);
        var getEquipmentGeometryTask = GetItEquipmentGeometryByAddressAsync(address, ct);

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

    private async Task<Result<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>> GetItEquipmentGeometryByAddressAsync(
        Address address,
        CancellationToken ct)
    {
        const string sql = """
            SELECT
                      bie.inventory_number as id
                    , bie.geometry as geometry
                    , b.geometry as building_basement_geometry
              FROM buildings_it_equipment bie
              JOIN buildings_levels bl ON bl.level_id = bl.id
              JOIN buildings b ON b.id = bl.building_id
             WHERE b.address = @address;
        """;
        var param = new NpgsqlParameter("address", address.ToString());

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<IDictionary<string, ItEquipmentGeometry>, ErrorMessage>(openConResult.Error);
        }
        
        await using var conn = openConResult.Value!;
        var result = new Dictionary<string, ItEquipmentGeometry>();
        try
        {
            await using var buildingInfoCommand = new NpgsqlCommand(sql, conn);
            buildingInfoCommand.Parameters.Add(param);
            
            var reader = await buildingInfoCommand.ExecuteReaderAsync(ct);

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