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
                id UUID PRIMARY KEY,
                building_id UUID NOT NULL REFERENCES buildings(id) ON DELETE CASCADE,
                name TEXT NOT NULL,
                CONSTRAINT uk_buildings_levels UNIQUE(building_id, name)
            );

            CREATE TABLE buildings_rooms (
                id INTEGER PRIMARY KEY,
                level_id UUID NOT NULL,
                type SMALLINT NOT NULL,
                architectural_id TEXT NOT NULL,
                geometry JSONB NOT NULL,
                name TEXT,
                CONSTRAINT uk_rooms UNIQUE (level_id, architectural_id),
                CONSTRAINT fk_rooms_buildings_levels FOREIGN KEY (level_id)
                    REFERENCES buildings_levels(id) ON DELETE CASCADE
            );

            CREATE TABLE buildings_walls (
                id UUID PRIMARY KEY,
                level_id UUID NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT fk_walls_buildings_levels FOREIGN KEY (level_id)
                    REFERENCES buildings_levels(id) ON DELETE CASCADE
            );

            CREATE TABLE buildings_it_equipment (
                inventory_number TEXT NOT NULL PRIMARY KEY,
                room_id INTEGER NOT NULL,
                geometry JSONB NOT NULL,
                CONSTRAINT fk_buildings_it_equipment FOREIGN KEY (room_id) REFERENCES buildings_rooms(id) ON DELETE CASCADE
            );

            CREATE TABLE users (
              id UUID PRIMARY KEY,
              email VARCHAR(320) NOT NULL UNIQUE
            );

            CREATE TABLE users_to_user_roles (
                user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                role_type SMALLINT NOT NULL,
                CONSTRAINT uk_single_role_of_same_type UNIQUE(user_id, role_type)
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