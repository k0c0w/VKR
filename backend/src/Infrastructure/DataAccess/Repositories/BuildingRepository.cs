using System.Collections.Frozen;
using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Npgsql;
using ResultMonad;

namespace DataAccess.Repositories;

internal sealed class BuildingRepository(
    NpgsqlDataSource dataSource,
    ILogger<IBuildingRepository> logger,
    IItEquipmentRepository itEquipmentCatalogue)
    : EntitiesWithJsonbRepositoryBase(dataSource, logger), IBuildingRepository
{
    private static int CachedCapacity = 4;

    public async Task<ResultWithError<ErrorMessage>> AddAsync(Building building, CancellationToken ct)
    {
        var insertBuildingCommand = InsertCommand(building);
        var insertLevelsCommand = InsertCommand(building.Id, building.Levels);

        var walls = new List<Wall>();
        var rooms = new List<Room>();
        var equipment = new List<ItEquipment>();
        foreach (var level in building.Levels)
        {
            walls.AddRange(level.Walls);
            rooms.AddRange(level.Rooms);
            equipment.AddRange(level.Rooms.SelectMany(x => x.ItEquipments));
        }

        var insertWallsCommand = walls.Count > 0 ? InsertCommand(walls) : default;
        var insertRoomsCommand = rooms.Count > 0 ? InsertCommand(rooms) : default;
        var insertEquipmentCommand = equipment.Count > 0 ? InsertCommand(equipment) : default;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value!;
        await using var commandsBatch = conn.CreateBatch();
        commandsBatch.BatchCommands.Add(insertBuildingCommand);
        commandsBatch.BatchCommands.Add(insertLevelsCommand);
        if (insertRoomsCommand is not null)
        {
            commandsBatch.BatchCommands.Add(insertRoomsCommand);
        }

        if (insertWallsCommand is not null)
        {
            commandsBatch.BatchCommands.Add(insertWallsCommand);
        }

        if (insertEquipmentCommand is not null)
        {
            commandsBatch.BatchCommands.Add(insertEquipmentCommand);
        }

        try
        {
            await commandsBatch.ExecuteNonQueryAsync(ct);
            return ResultWithError.Ok<ErrorMessage>();
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return ResultWithError.Fail(ErrorMessage.RepositorySpecificErrors.AddError);
        }
    }

    public async Task<Result<(Guid BuildingId, Location Location, string BuildingName)[], ErrorMessage>>
        GetAllBuildingInformationAsync(CancellationToken ct)
    {
        const string sql = """
                               SELECT 
                                        b.id
                                      , b.name
                                      , b.region
                                      , b.address
                                 FROM buildings b;
                           """;

        var buildingInfos = new List<(Guid, Location, string)>(CachedCapacity);
        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<(Guid, Location, string)[], ErrorMessage>(openConResult.Error);
        }

        await using var conn = openConResult.Value!;
        await using var command = new NpgsqlCommand(sql, conn);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var buildingId = reader.GetGuid(0);
                var buildingName = reader.GetString(1);
                var region = reader.GetString(2);
                var address = reader.IsDBNull(3) ? default : reader.GetString(3);

                buildingInfos.Add((buildingId, new Location(region, address), buildingName));
            }

            CachedCapacity = buildingInfos.Count;

            return Result.Ok<(Guid, Location, string)[], ErrorMessage>(buildingInfos.ToArray());
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<(Guid, Location, string )[], ErrorMessage>(
                ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    public async Task<Result<Building, ErrorMessage>> GetBuildingAsync(IBuildingRepository.BuildingFilter filter,
        CancellationToken ct)
    {
        const string buildingInfoSqlFormat = """
                                                 SELECT   b.id
                                                        , b.name
                                                        , b.region
                                                        , b.address
                                                        , b.geometry
                                                   FROM buildings b
                                                  WHERE b.id = @filter
                                                  LIMIT 1;
                                             """;

        var buildingFilterParam = new NpgsqlParameter("filter", filter.Id.Value);

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<Building, ErrorMessage>(openConResult.Error);
        }

        await using var conn = openConResult.Value!;

        Guid buildingId;
        string buildingName;
        Location buildingLocation;
        Polygon buildingBasementGeometry;
        IDictionary<uint, Level> allBuildingLevels;
        try
        {
            await using (var buildingInfoCommand = new NpgsqlCommand(buildingInfoSqlFormat, conn))
            {
                buildingInfoCommand.Parameters.Add(buildingFilterParam);
                await using var buildingReader = await buildingInfoCommand.ExecuteReaderAsync(ct);

                if (!await buildingReader.ReadAsync(ct))
                {
                    return Result.Fail<Building, ErrorMessage>(ErrorMessage.EntityNotfoundError);
                }

                buildingId = buildingReader.GetGuid(0);
                buildingName = buildingReader.GetString(1);
                var buildingRegion = buildingReader.GetString(2);
                var address = buildingReader.IsDBNull(3) ? default : buildingReader.GetString(3);

                buildingLocation = new Location(buildingRegion, address);
                buildingBasementGeometry = buildingReader.GetFieldValue<Polygon>(4);
            }

            allBuildingLevels = (await FetchBuildingEmptyLevelsAsync(conn, buildingId, ct))
                .ToDictionary(k => k.Id.Number, v => v);

            await EnrichLevelWithBuildingStructuresAsync(conn, buildingId, allBuildingLevels, ct);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return Result.Fail<Building, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }

        var rooms = allBuildingLevels.Values.SelectMany(l => l.Rooms);
        var itEquipmentResult =
            await itEquipmentCatalogue.GetItEquipmentByRoomIdsAsync(rooms.Select(x => x.Id).ToArray(), ct);
        if (itEquipmentResult.IsFailure)
        {
            return Result.Fail<Building, ErrorMessage>(itEquipmentResult.Error);
        }

        var itEquipment = itEquipmentResult.Value
            .GroupBy(x => x.Description.LocationAudienceCatalogueId)
            .ToDictionary(k => k.Key, v => v);

        foreach (var room in rooms)
        {
            var roomEquipment = itEquipment.TryGetValue(room.Id, out var value) ? value.ToArray() : [];
            foreach (var instance in roomEquipment)
            {
                room.AddEquipment(instance);
            }
        }

        return Result.Ok<Building, ErrorMessage>(Building.CreateExistingBuildingInstance(
            buildingId,
            buildingLocation,
            buildingName,
            buildingBasementGeometry,
            allBuildingLevels.Values));
    }

    public async Task<ResultWithError<ErrorMessage>> UpdateAsync(Building building, CancellationToken ct)
    {
        const string updateGeometrySql = $"""
                                             UPDATE buildings building
                                                SET   geometry = @{nameof(building.BasementGeometry)}
                                                    , address = @{nameof(building.Location.Address)}
                                                    , region = @{nameof(building.Location.Region)}
                                                    , name = @{nameof(building.Name)}
                                              WHERE building.id = @{nameof(building.Id)};  
                                          """;

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value;
        await using var command = new NpgsqlCommand(updateGeometrySql, conn);
        command.Parameters.AddWithValue(nameof(building.BasementGeometry), NpgsqlTypes.NpgsqlDbType.Jsonb,
            Serialize(building.BasementGeometry));
        command.Parameters.AddWithValue(nameof(building.Location.Address), building.Location.Address);
        command.Parameters.AddWithValue(nameof(building.Location.Region), building.Location.Region);
        command.Parameters.AddWithValue(nameof(building.Name), building.Name);
        command.Parameters.AddWithValue(nameof(building.Id), building.Id);
        try
        {
            var affectedRowsCount = await command.ExecuteNonQueryAsync(ct);

            return affectedRowsCount > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return ResultWithError.Fail(ErrorMessage.DomainError("Произошла ошибка при обновлении здания."));
        }
    }

    public async Task<Result<bool, ErrorMessage>> AnyBuildingWithSameNameAtRegionAsync(string name, string region,
        CancellationToken ct)
    {
        const string existenceSql = """
                                    SELECT 1
                                      FROM buildings b
                                     WHERE b.name = @name and b.region = @region
                                     LIMIT 1;
                                    """;

        var nameParam = new NpgsqlParameter("name", name);
        var addressParam = new NpgsqlParameter("region", region);

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<bool, ErrorMessage>(openConResult.Error);
        }

        await using var conn = openConResult.Value!;
        await using var command = new NpgsqlCommand(existenceSql, conn);
        command.Parameters.Add(nameParam);
        command.Parameters.Add(addressParam);

        try
        {
            var scalar = await command.ExecuteScalarAsync(ct);

            return Result.Ok<bool, ErrorMessage>(scalar is not null);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<bool, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    private static async Task<List<Level>> FetchBuildingEmptyLevelsAsync(NpgsqlConnection connection, Guid buildingId,
        CancellationToken ct)
    {
        const string levelsSql = $"""
                                     SELECT 
                                              bl.building_id
                                            , bl.number
                                            , bl.name
                                       FROM buildings_levels bl
                                      WHERE bl.building_id = @{nameof(buildingId)};
                                  """;
        await using var command = new NpgsqlCommand(levelsSql, connection);
        command.Parameters.AddWithValue(nameof(buildingId), buildingId);

        var levels = new List<Level>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var buildingIdFromDb = reader.GetGuid(0);
            var number = reader.GetInt32(1);
            var name = reader.GetString(2);

            var level = Level.CreateExistingLevel(buildingIdFromDb, (uint)number, name, [], []);
            levels.Add(level);
        }

        return levels;
    }

    private static async Task EnrichLevelWithBuildingStructuresAsync(NpgsqlConnection connection,
        Guid buildingId,
        IDictionary<uint, Level> knownLevels,
        CancellationToken ct)
    {
        const string roomsOrWallsSql = $"""
                                            SELECT 
                                                       B'0'::BIT AS is_wall_feature_type
                                                     , r.level_number AS level_number
                                                     , r.id AS room_id
                                                     , r.type AS room_type
                                                     , r.geometry AS room_geometry
                                                     , r.name AS room_name
                                                     , NULL AS wall_id
                                                     , NULL AS wall_geometry
                                              FROM buildings_rooms r
                                             WHERE r.building_id = @{nameof(buildingId)}
                                        
                                             UNION ALL
                                        
                                            SELECT 
                                                       B'1'::BIT AS is_wall_feature_type
                                                     , w.level_number AS level_number
                                                     , NULL AS room_id
                                                     , NULL AS room_type
                                                     , NULL AS room_geometry
                                                     , NULL AS room_name
                                                       , w.id AS wall_id
                                                       , w.geometry AS wall_geometry
                                              FROM buildings_walls w
                                             WHERE w.building_id = @{nameof(buildingId)};
                                        """;

        await using var command = new NpgsqlCommand(roomsOrWallsSql, connection);
        command.Parameters.AddWithValue(nameof(buildingId), buildingId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var isWall = reader.GetBoolean(0);
            var levelNumber = reader.GetInt32(1);
            if (!knownLevels.TryGetValue((uint)levelNumber, out var level))
            {
                continue;
            }

            if (isWall)
            {
                var id = reader.GetGuid(6);
                var geometry = reader.GetFieldValue<LineString>(7);

                Wall.CreateExistingRoomAtLevel(level, id, geometry);
            }
            else
            {
                var id = reader.GetInt64(2);
                var type = (RoomType)reader.GetInt16(3);
                var geometry = reader.GetFieldValue<Polygon>(4);
                var name = reader.IsDBNull(5) ? null : reader.GetString(5);

                var description = new RoomDescription
                {
                    Geometry = geometry,
                    Type = type,
                    Name = name,
                };

                Room.CreateExistingRoomAtLevel(level, id, description, []);
            }
        }
    }

    private NpgsqlBatchCommand InsertCommand(Building building)
    {
        const string insertBuildingSql = """
                                              INSERT INTO buildings (id, name, region, address, geometry)
                                              VALUES (@id, @name, @region, @address, @geometry ::jsonb);
                                         """;

        var insertBuildingCommand = new NpgsqlBatchCommand(insertBuildingSql)
        {
            Parameters =
            {
                new NpgsqlParameter("id", building.Id),
                new NpgsqlParameter("name", building.Name),
                new NpgsqlParameter("region", building.Location.Region),
                new NpgsqlParameter("address",
                    string.IsNullOrEmpty(building.Location.Address) ? null : building.Location.Address),
            }
        };
        insertBuildingCommand.Parameters.AddWithValue("geometry", NpgsqlTypes.NpgsqlDbType.Jsonb,
            Serialize(building.BasementGeometry));

        return insertBuildingCommand;
    }

    private static NpgsqlBatchCommand InsertCommand(Guid buildingId, IReadOnlyCollection<Level> levels)
    {
        const string insertSqlLevelsStart = """
                                                INSERT INTO buildings_levels (building_id, number, name)
                                                VALUES
                                            """;

        const int insertValuesCount = 2;
        var levelValuesSql = new string[levels.Count];
        var levelValuesParams = new NpgsqlParameter[levels.Count * insertValuesCount];
        foreach (var (index, level) in levels.Index())
        {
            var levelNumberParam = $"level_number_{index}";
            var nameParam = $"level_name_{index}";
            var offset = insertValuesCount * index;

            levelValuesSql[index] = $"(@{nameof(buildingId)},@{levelNumberParam},@{nameParam})";
            levelValuesParams[offset] = new NpgsqlParameter(levelNumberParam, (int)level.Id.Number);
            levelValuesParams[offset + 1] = new NpgsqlParameter(nameParam, level.Name);
        }

        var insertLevelsCommand = new NpgsqlBatchCommand($"{insertSqlLevelsStart} {string.Join(',', levelValuesSql)};");
        insertLevelsCommand.Parameters.AddRange(levelValuesParams);
        insertLevelsCommand.Parameters.AddWithValue(nameof(buildingId), buildingId);

        return insertLevelsCommand;
    }

    private NpgsqlBatchCommand InsertCommand(IReadOnlyList<Wall> walls)
    {
        const string insertSqlWallsStart = """
                                              INSERT INTO buildings_walls (id, building_id, level_number, geometry)
                                              VALUES
                                           """;

        var uniqueLevelParams = walls
            .Where(x => x.BelongsToLevel is not null)
            .Select(x => x.BelongsToLevel!.Id.Number)
            .DistinctBy(x => x)
            .ToFrozenDictionary(x => x, v => new NpgsqlParameter($"level_number_{v}", (int)v));

        var buildingId = walls[0].BelongsToLevel!.Id.BuildingId;
        var buildingIdParam = new NpgsqlParameter("building_id", buildingId);

        var wallValuesSql = new List<string>(walls.Count);
        var wallValuesParams = new List<NpgsqlParameter>(2 * walls.Count);
        foreach (var (index, wall) in walls.Index())
        {
            var idParam = $"wall_id_{index}";
            var geometryParam = $"wall_geometry_{index}";
            var levelParam = uniqueLevelParams[wall.BelongsToLevel!.Id.Number].ParameterName;

            wallValuesSql.Add($"(@{idParam},@{buildingIdParam.ParameterName},@{levelParam},@{geometryParam}::jsonb)");
            wallValuesParams.Add(new NpgsqlParameter(idParam, wall.Id));
            wallValuesParams.Add(new NpgsqlParameter(geometryParam, Serialize(wall.Geometry)));
        }

        var insertWallCommand = new NpgsqlBatchCommand($"{insertSqlWallsStart} {string.Join(',', wallValuesSql)};");
        foreach (var levelParam in uniqueLevelParams.Values)
        {
            insertWallCommand.Parameters.Add(levelParam);
        }

        foreach (var valueParam in wallValuesParams)
        {
            insertWallCommand.Parameters.Add(valueParam);
        }

        insertWallCommand.Parameters.Add(buildingIdParam);

        return insertWallCommand;
    }

    private NpgsqlBatchCommand InsertCommand(IReadOnlyList<Room> rooms)
    {
        const string insertSqlRoomsStart = """
                                              INSERT INTO buildings_rooms (id, building_id, level_number, type, geometry, name)
                                              VALUES
                                           """;
        var uniqueLevelParams = rooms
            .Where(x => x.BelongsToLevel is not null)
            .Select(x => x.BelongsToLevel!.Id.Number)
            .DistinctBy(x => x)
            .ToFrozenDictionary(x => x, v => new NpgsqlParameter($"level_number_{v}", (int)v));
        var buildingId = rooms[0].BelongsToLevel!.Id.BuildingId;
        var buildingIdParam = new NpgsqlParameter("building_id", buildingId);

        var roomValuesSql = new List<string>(rooms.Count);
        var roomValuesParams = new List<NpgsqlParameter>(4 * rooms.Count);
        for (var index = 0; index < rooms.Count; index++)
        {
            var room = rooms[index];

            var levelParam = uniqueLevelParams[room.BelongsToLevel.Id.Number].ParameterName;
            var idParam = $"room_id_{index}";
            var typeParam = $"room_type_{index}";
            var geometryParam = $"room_geometry_{index}";
            var nameParam = $"room_name_{index}";

            roomValuesSql.Add(
                $"(@{idParam},@{buildingIdParam.ParameterName},@{levelParam},@{typeParam},@{geometryParam} ::jsonb,@{nameParam})");
            roomValuesParams.Add(new NpgsqlParameter(idParam, room.Id));
            roomValuesParams.Add(new NpgsqlParameter(typeParam, (short)room.Type));
            roomValuesParams.Add(new NpgsqlParameter(geometryParam, Serialize(room.Geometry)));
            roomValuesParams.Add(new NpgsqlParameter(nameParam, room.Name ?? ""));
        }

        var insertRoomsCommand = new NpgsqlBatchCommand($"{insertSqlRoomsStart} {string.Join(',', roomValuesSql)};");
        foreach (var levelParam in uniqueLevelParams.Values)
        {
            insertRoomsCommand.Parameters.Add(levelParam);
        }

        foreach (var valueParam in roomValuesParams)
        {
            insertRoomsCommand.Parameters.Add(valueParam);
        }

        insertRoomsCommand.Parameters.Add(buildingIdParam);

        return insertRoomsCommand;
    }

    private NpgsqlBatchCommand InsertCommand(IReadOnlyList<ItEquipment> itEquipments)
    {
        const string insetItEquipmentBase = """
                                                INSERT INTO buildings_it_equipment (inventory_number, room_id, geometry)
                                                VALUES
                                            """;

        const int insertValuesCount = 3;
        var sqlParamsText = new string[itEquipments.Count];
        var sqlParams = new NpgsqlParameter[itEquipments.Count * insertValuesCount];
        for (var index = 0; index < itEquipments.Count; index++)
        {
            var equipment = itEquipments[index];
            var idParam = $"{nameof(ItEquipment.Id)}{index}";
            var roomIdParam = $"{nameof(ItEquipment.Description.LocationAudienceCatalogueId)}{index}";
            var geometry = $"{nameof(ItEquipment.Geometry)}{index}";
            var offset = insertValuesCount * index;

            sqlParamsText[index] = $"(@{idParam},@{roomIdParam},@{geometry}::jsonb)";
            sqlParams[offset] = new NpgsqlParameter(idParam, equipment.Id);
            sqlParams[offset + 1] = new NpgsqlParameter(roomIdParam, equipment.Description.LocationAudienceCatalogueId);
            sqlParams[offset + 2] = new NpgsqlParameter(geometry, Serialize(equipment.Geometry));
        }

        var insertEquipmentCommand =
            new NpgsqlBatchCommand($"{insetItEquipmentBase} {string.Join(',', sqlParamsText)};");
        insertEquipmentCommand.Parameters.AddRange(sqlParams);

        return insertEquipmentCommand;
    }

    public async Task<ResultWithError<ErrorMessage>> RemoveAsync(Building entity, CancellationToken ct)
    {
        const string deleteEquipmentSql = """
                                              DELETE FROM buildings_it_equipment 
                                               WHERE room_id IN (SELECT id FROM buildings_rooms WHERE building_id = @buildingId);
                                          """;

        const string deleteRoomsSql = """
                                          DELETE FROM buildings_rooms 
                                           WHERE building_id = @buildingId;
                                      """;

        const string deleteWallsSql = """
                                          DELETE FROM buildings_walls 
                                           WHERE building_id = @buildingId;
                                      """;

        const string deleteLevelsSql = """
                                           DELETE FROM buildings_levels 
                                            WHERE building_id = @buildingId;
                                       """;

        const string deleteBuildingSql = """
                                             DELETE FROM buildings 
                                              WHERE id = @buildingId;
                                         """;
        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return ResultWithError.Fail(openConResult.Error);
        }

        await using var conn = openConResult.Value!;
        await using var batch = conn.CreateBatch();
        batch.BatchCommands.Add(new NpgsqlBatchCommand(deleteEquipmentSql)
        {
            Parameters = { new NpgsqlParameter("buildingId", entity.Id) }
        });
        batch.BatchCommands.Add(new NpgsqlBatchCommand(deleteRoomsSql)
        {
            Parameters = { new NpgsqlParameter("buildingId", entity.Id) }
        });
        batch.BatchCommands.Add(new NpgsqlBatchCommand(deleteWallsSql)
        {
            Parameters = { new NpgsqlParameter("buildingId", entity.Id) }
        });
        batch.BatchCommands.Add(new NpgsqlBatchCommand(deleteLevelsSql)
        {
            Parameters = { new NpgsqlParameter("buildingId", entity.Id) }
        });
        batch.BatchCommands.Add(new NpgsqlBatchCommand(deleteBuildingSql)
        {
            Parameters = { new NpgsqlParameter("buildingId", entity.Id) }
        });

        try
        {
            var affectedRows = await batch.ExecuteNonQueryAsync(ct);

            return affectedRows > 0 ? ResultWithError.Ok<ErrorMessage>() : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return ResultWithError.Fail(ErrorMessage.DomainError($"Не удалось удалить план {entity.Id}."));
        }
    }
}