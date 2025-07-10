using FluentMigrator;

namespace Migrations.Migrations;

[Migration(1)]
public class Init : Migration
{
    public override void Up()
    {
        const string sql = """
            CREATE TABLE buildings (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                region TEXT NOT NULL,
                address TEXT,
                geometry JSONB NOT NULL,
                CONSTRAINT uk_buildings_at_region UNIQUE (region, name, address)
            );
            CREATE INDEX idx_buildings_region ON buildings (region);

            CREATE TABLE buildings_levels (
                building_id UUID NOT NULL REFERENCES buildings(id) ON DELETE CASCADE,
                number INTEGER NOT NULL,
                name TEXT NOT NULL,
                
                CONSTRAINT pk_buildings_levels PRIMARY KEY (building_id, number)
            );

            CREATE TABLE buildings_rooms (
                id INTEGER PRIMARY KEY,
                building_id UUID NOT NULL,
                level_number INTEGER NOT NULL,
                type SMALLINT NOT NULL,
                geometry JSONB NOT NULL,
                name TEXT,
                CONSTRAINT fk_rooms_buildings_levels FOREIGN KEY (building_id, level_number)
                    REFERENCES buildings_levels(building_id, number) ON DELETE CASCADE
            );

            CREATE TABLE buildings_walls (
                id UUID PRIMARY KEY,
                building_id UUID NOT NULL,
                level_number INTEGER NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT fk_walls_buildings_levels FOREIGN KEY (building_id, level_number)
                    REFERENCES buildings_levels(building_id, number) ON DELETE CASCADE
            );

            CREATE TABLE buildings_it_equipment (
                inventory_number TEXT NOT NULL PRIMARY KEY,
                room_id INTEGER NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT fk_buildings_it_equipment FOREIGN KEY (room_id) REFERENCES buildings_rooms (id) ON DELETE CASCADE
            );

            CREATE TABLE users (
                email TEXT primary key,
                roles SMALLINT[] NOT NULL
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = """
            DROP TABLE buildings_it_equipment;
            DROP TABLE buildings_walls;
            DROP TABLE buildings_rooms;
            DROP TABLE buildings_levels;
            DROP TABLE buildings;

            DROP TABLE users_to_user_roles;
            DROP TABLE users;
        """;

        Execute.Sql(sql);
    }
}