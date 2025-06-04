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
        var insertBuildingCommand = InsertBuildingCommand(building);
        var insertLevelsCommand = InsertLevelsCommand(building.Id, building.Levels);
        var insertWallsCommand = InsertWallsCommandOrDefault(building.Levels.SelectMany(x => x.Walls).ToArray());
        var insertRoomsCommand = InsertRoomsCommandOrDefault(building.Levels.SelectMany(x => x.Rooms).ToArray());

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

    public async Task<Result<(Guid BuildingId, Address Address, string BuildingName)[], ErrorMessage>> GetAllBuildingInformationAsync(CancellationToken ct)
    {
        const string sql = """
                               SELECT 
                                        b.id
                                      , b.name
                                      , b.address
                                 FROM buildings b
                                 JOIN (SELECT
                                                buildings_levels.building_id as building_id
                                              , count(buildings_levels.name) as levels_count
                                         FROM buildings_levels
                                        GROUP BY buildings_levels.building_id
                                       ) bl ON bl.building_id = b.id;
                           """;

        var buildingInfos = new List<(Guid BuildingId, Address Address, string BuildingName)>(CachedCapacity);
        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<(Guid BuildingId, Address Address, string)[], ErrorMessage>(openConResult.Error);
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
                var addressString = reader.GetString(2);

                buildingInfos.Add((buildingId, Address.FromString(addressString), buildingName));
            }

            CachedCapacity = buildingInfos.Count;

            return Result.Ok<(Guid BuildingId, Address Address, string BuildingName)[], ErrorMessage>(buildingInfos.ToArray());
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<(Guid, Address, string )[], ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    public async Task<Result<Building, ErrorMessage>> GetBuildingAsync(IBuildingRepository.BuildingFilter filter,
        CancellationToken ct)
    {
        const string buildingInfoSqlFormat = """
                                                 SELECT   b.id
                                                        , b.name
                                                        , b.address
                                                        , b.geometry
                                                   FROM buildings b
                                                  WHERE b.{0} = @filter
                                                  LIMIT 1;
                                             """;
        var buildingInfoSql = string.Format(buildingInfoSqlFormat, filter.Id.HasValue ? "id" : "address");
        var buildingFilterParam = new NpgsqlParameter("filter",
            filter.Id.HasValue
                ? filter.Id.Value
                : filter.Address?.ToString() ?? throw new ArgumentException("Provide any building filter."));

        var openConResult = await GetOpenedConnectionAsync(ct);
        if (openConResult.IsFailure)
        {
            return Result.Fail<Building, ErrorMessage>(openConResult.Error);
        }

        await using var conn = openConResult.Value!;

        Guid buildingId;
        string buildingName;
        Address buildingAddress;
        Polygon buildingBasementGeometry;
        IDictionary<Guid, Level> allBuildingLevels;
        try
        {
            await using (var buildingInfoCommand = new NpgsqlCommand(buildingInfoSql, conn))
            {
                buildingInfoCommand.Parameters.Add(buildingFilterParam);
                await using var buildingReader = await buildingInfoCommand.ExecuteReaderAsync(ct);

                if (!await buildingReader.ReadAsync(ct))
                {
                    return Result.Fail<Building, ErrorMessage>(ErrorMessage.EntityNotfoundError);
                }

                buildingId = buildingReader.GetGuid(0);
                buildingName = buildingReader.GetString(1);
                var buildingAddressString = buildingReader.GetString(2);
                buildingAddress = Address.FromString(buildingAddressString);
                buildingBasementGeometry = buildingReader.GetFieldValue<Polygon>(3);
            }
            
            allBuildingLevels = (await FetchBuildingEmptyLevelsAsync(conn, buildingId, ct))
                .ToDictionary(k => k.Id, v=>v);

            await EnrichLevelWithBuildingStructuresAsync(conn, buildingId, allBuildingLevels, ct);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);

            return Result.Fail<Building, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }

        var rooms = allBuildingLevels.Values.SelectMany(l => l.Rooms);
        var itEquipmentResult = await itEquipmentCatalogue.GetItEquipmentByRoomIdsAsync(rooms.Select(x => x.Id).ToArray(), ct);
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
            buildingAddress,
            buildingName,
            buildingBasementGeometry, 
            allBuildingLevels.Values));
    }

    public async Task<Result<bool, ErrorMessage>> AnyBuildingWithAddressOrNameAsync(string name, Address address, CancellationToken ct)
    {
        const string existenceSql = """
                                    SELECT 1
                                      FROM buildings b
                                     WHERE b.name = $name or b.address = $address
                                     LIMIT 1;
                                    """;
        
        var nameParam = new NpgsqlParameter("name", name);
        var addressParam = new NpgsqlParameter("address", address.ToString());

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
                                              bl.id
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
            var id = reader.GetGuid(0);
            var name = reader.GetString(1);
            var level = Level.CreateExistingLevel(id, buildingId, name, [], []);

            levels.Add(level);
        }

        return levels;
    }

    private static async Task EnrichLevelWithBuildingStructuresAsync(NpgsqlConnection connection,
        Guid buildingId,
        IDictionary<Guid, Level> knownLevels,
        CancellationToken ct)
    {
        const string roomsOrWallsSql = $"""
                                            SELECT 
                                                       B'0'::BIT AS is_wall_feature_type
                                                     , bl.id AS level_id
                                                 
                                                     , r.id AS room_id
                                                     , r.type AS room_type
                                                     , r.architectural_id AS room_architectural_id
                                                     , r.geometry AS room_geometry
                                                     , r.name AS room_name
                                                 
                                                     , NULL AS wall_id
                                                     , NULL AS wall_geometry
                                              FROM buildings_levels bl
                                             RIGHT JOIN buildings_rooms r ON r.building_id = bl.building_id AND r.level_id = bl.id
                                             WHERE bl.building_id = @{nameof(buildingId)}
                                        
                                             UNION ALL
                                        
                                            SELECT 
                                                       B'1'::BIT AS is_wall_feature_type
                                                     , bl.id AS level_id
                                                 
                                                     , NULL AS room_id
                                                     , NULL AS room_type
                                                     , NULL AS room_architectural_id
                                                     , NULL AS room_geometry
                                                     , NULL AS room_name
                                                       
                                                     , w.id AS wall_id
                                                     , w.geometry AS wall_geometry
                                              FROM buildings_levels bl
                                              RIGHT JOIN buildings_walls w ON w.building_id = bl.building_id AND w.level_id = bl.id
                                             WHERE bl.building_id = @{nameof(buildingId)};
                                        """;

        await using var command = new NpgsqlCommand(roomsOrWallsSql, connection);
        command.Parameters.AddWithValue(nameof(buildingId), buildingId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var isWall = reader.GetBoolean(0);
            var levelId = reader.GetGuid(1);
            if (!knownLevels.TryGetValue(levelId, out var level))
            {
                continue;
            }

            if (isWall)
            {
                var id = reader.GetGuid(7);
                var geometry = reader.GetFieldValue<LineString>(8);

                Wall.CreateExistingRoomAtLevel(level, id, geometry);
            }
            else
            {
                var id = reader.GetInt64(2);
                var type = (RoomType)reader.GetInt16(3);
                var archId = reader.GetString(4);
                var geometry = reader.GetFieldValue<Polygon>(5);
                var name = reader.GetString(6);

                var description = new RoomDescription
                {
                    Geometry = geometry,
                    Type = type,
                    ArchitectualId = archId,
                    Name = name,
                };

                Room.CreateExistingRoomAtLevel(level, id, description, []);
            }
        }
    }

    private NpgsqlBatchCommand InsertBuildingCommand(Building building)
    {
        const string insertBuildingSql = """
                                              INSERT INTO buildings (id, address, geometry)
                                              VALUES (@id, @address, @geometry ::jsonb);
                                         """;

        var insertBuildingCommand = new NpgsqlBatchCommand(insertBuildingSql)
        {
            Parameters =
            {
                new NpgsqlParameter("id", building.Id),
                new NpgsqlParameter("address", building.Address.ToString()),
                new NpgsqlParameter("geometry", Serialize(building.BasementGeometry))
            }
        };

        return insertBuildingCommand;
    }

    private static NpgsqlBatchCommand InsertLevelsCommand(Guid buildingId, IReadOnlyCollection<Level> levels)
    {
        const string insertSqlLevelsStart = """
                                                INSERT INTO buildings_levels (building_id, name)
                                                VALUES
                                            """;

        var levelValuesSql = new string[levels.Count];
        var levelValuesParams = new NpgsqlParameter[levels.Count * 2];
        foreach (var (index, level) in levels.Index())
        {
            var buildingIdParam = $"level_building_id_{index}";
            var nameParam = $"level_name_{index}";
            var offset = 2 * index;

            levelValuesSql[index] = $"(@{buildingIdParam},@{nameParam})";
            levelValuesParams[offset] = new NpgsqlParameter(buildingIdParam, buildingId);
            levelValuesParams[offset + 1] = new NpgsqlParameter(nameParam, level.Name);
        }

        var insertLevelsCommand =
            new NpgsqlBatchCommand($"{insertSqlLevelsStart} {string.Join(',', levelValuesSql)};");
        insertLevelsCommand.Parameters.AddRange(levelValuesParams);

        return insertLevelsCommand;
    }

    private NpgsqlBatchCommand? InsertWallsCommandOrDefault(Wall[] walls)
    {
        if (walls.Length == 0)
        {
            return default;
        }
        
        const string insertSqlWallsStart = """
                                              INSERT INTO buildings_walls (id, level_id, geometry)
                                              VALUES
                                           """;

        var wallValuesSql = new List<string>();
        var wallValuesParams = new List<NpgsqlParameter>();
        foreach (var (index, wall) in walls.Index())
        {
            var idParam = $"wall_id_{index}";
            var levelParam = $"wall_level_id_{index}";
            var geometryParam = $"wall_geometry_{index}";

            wallValuesSql.Add($"(@{idParam},@{levelParam},@{geometryParam} ::jsonb)");
            wallValuesParams.Add(new NpgsqlParameter(idParam, wall.Id));
            wallValuesParams.Add(new NpgsqlParameter(levelParam, wall.BelongsToLevelId));
            wallValuesParams.Add(new NpgsqlParameter(geometryParam, Serialize(wall.Geometry)));
        }

        var insertWallCommand = new NpgsqlBatchCommand($"{insertSqlWallsStart} {string.Join(',', wallValuesSql)};");
        foreach (var valueParam in wallValuesParams)
        {
            insertWallCommand.Parameters.Add(valueParam);
        }

        return insertWallCommand;
    }

    private NpgsqlBatchCommand? InsertRoomsCommandOrDefault(Room[] rooms)
    {
        if (rooms.Length == 0)
        {
            return default;
        }
        
        const string insertSqlRoomsStart = """
                                              INSERT INTO buildings_rooms (id, building_id, level, type, architectural_id, geometry, name)
                                              VALUES
                                           """;

        var roomValuesSql = new List<string>();
        var roomValuesParams = new List<NpgsqlParameter>();
        foreach (var (index, room) in rooms.Index())
        {
            var idParam = $"room_id_{index}";
            var levelParam = $"room_level_id_{index}";
            var typeParam = $"room_type_{index}";
            var archIdParam = $"room_architectural_id_{index}";
            var geometryParam = $"room_geometry_{index}";
            var nameParam = $"room_name_{index}";

            roomValuesSql.Add(
                $"(@{idParam},@{levelParam},@{typeParam},@{archIdParam},@{geometryParam} ::jsonb,@{nameParam})");
            roomValuesParams.Add(new NpgsqlParameter(idParam, room.Id));
            roomValuesParams.Add(new NpgsqlParameter(levelParam, room.BelongsToLevelId));
            roomValuesParams.Add(new NpgsqlParameter(typeParam, (short)room.Type));
            roomValuesParams.Add(new NpgsqlParameter(archIdParam, room.ArchitectualId));
            roomValuesParams.Add(new NpgsqlParameter(geometryParam, Serialize(room.Geometry)));
            roomValuesParams.Add(new NpgsqlParameter(nameParam, room.Name ?? ""));
        }

        var insertRoomsCommand = new NpgsqlBatchCommand($"{insertSqlRoomsStart} {string.Join(',', roomValuesSql)};");
        foreach (var valueParam in roomValuesParams)
        {
            insertRoomsCommand.Parameters.Add(valueParam);
        }

        return insertRoomsCommand;
    }
}