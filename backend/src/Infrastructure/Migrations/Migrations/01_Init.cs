using FluentMigrator;

namespace Migrations.Migrations;

[Migration(1)]
public class Init : Migration
{
    public override void Up()
    {
        const string sql = """
            CREATE TABLE buildings (
                id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                address TEXT NOT NULL,
                geometry JSONB NOT NULL
            );

            CREATE TABLE buildings_levels (
                building_id INTEGER NOT NULL REFERENCES buildings(id) ON DELETE CASCADE,
                number INTEGER NOT NULL,
                name TEXT NOT NULL,
                CONSTRAINT pk_buildings_levels PRIMARY KEY (building_id, number)
            );

            CREATE TABLE rooms (
                id UUID PRIMARY KEY,
                building_id INTEGER NOT NULL,
                level INTEGER NOT NULL,
                type SMALLINT NOT NULL,
                architectural_id TEXT NOT NULL,
                geometry JSONB NOT NULL,
                name TEXT,
                CONSTRAINT uk_rooms UNIQUE (building_id, level, architectural_id),
                CONSTRAINT fk_rooms_buildings_levels FOREIGN KEY (building_id, level)
                    REFERENCES buildings_levels(building_id, number) ON DELETE CASCADE
            );

            CREATE TABLE walls (
                id UUID PRIMARY KEY,
                building_id INTEGER NOT NULL,
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
                room_id UUID REFERENCES rooms(id) ON DELETE SET NULL
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = """
            DROP TABLE it_equipment;
            DROP TABLE walls;
            DROP TABLE rooms;
            DROP TABLE buildings_levels;
            DROP TABLE buildings;
        """;

        Execute.Sql(sql);
    }
}