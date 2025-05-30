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
                address TEXT NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT uk_buildings_address UNIQUE (address),
                CONSTRAINT uk_buildings_name UNIQUE (name)
            );

            CREATE TABLE buildings_levels (
                building_id UUID NOT NULL REFERENCES buildings(id) ON DELETE CASCADE,
                number INTEGER NOT NULL,
                name TEXT NOT NULL,
                CONSTRAINT pk_buildings_levels PRIMARY KEY (building_id, number)
            );

            CREATE TABLE buildings_rooms (
                id UUID PRIMARY KEY,
                building_id UUID NOT NULL,
                level INTEGER NOT NULL,
                type SMALLINT NOT NULL,
                architectural_id TEXT NOT NULL,
                geometry JSONB NOT NULL,
                name TEXT,
                CONSTRAINT uk_rooms UNIQUE (building_id, level, architectural_id),
                CONSTRAINT fk_rooms_buildings_levels FOREIGN KEY (building_id, level)
                    REFERENCES buildings_levels(building_id, number) ON DELETE CASCADE
            );

            CREATE TABLE buildings_walls (
                id UUID PRIMARY KEY,
                building_id UUID NOT NULL,
                level INTEGER NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT fk_walls_buildings_levels FOREIGN KEY (building_id, level)
                    REFERENCES buildings_levels(building_id, number) ON DELETE CASCADE
            );

            CREATE TABLE it_equipment (
                id UUID PRIMARY KEY,
                inventory_number TEXT NOT NULL,
                serial_number TEXT NOT NULL,
                name TEXT NOT NULL,
                room_id UUID REFERENCES buildings_rooms(id) ON DELETE SET NULL
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = """
            DROP TABLE it_equipment;
            DROP TABLE buildings_walls;
            DROP TABLE buildings_rooms;
            DROP TABLE buildings_levels;
            DROP TABLE buildings;
        """;

        Execute.Sql(sql);
    }
}