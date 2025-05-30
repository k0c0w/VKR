using Bogus;
using Domain.Aggregates;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;

namespace IntegrationTests.DatabaseTests.Fixtures;

public static class BuildingFixture
{
    private static readonly Faker NameFaker = new ();
    public static string GetRandomCompanyName() => NameFaker.Company.CompanyName();
    
    private static readonly Randomizer Randomizer = new ();
    private static string[] streetTypes = ["улица", "шоссе"];

    private static string GetStreetType() => streetTypes[Random.Shared.Next(streetTypes.Length)];
    
    internal static Building CreateTestBuilding(string city = "Казань")
    {
        var building = new Building(new Address(city, Randomizer.String(minChar:'а',maxChar:'я'), GetStreetType(), Random.Shared.Next(1, 150).ToString()),
            GetRandomCompanyName(),
            new Polygon([
                new LineString([
                    new Position(1, 1),
                    new Position(1, -1),
                    new Position(-1, -1),
                    new Position(-1, 1),
                    new Position(1, 1),
                ])
            ])
        );

        var level = building.Levels.Single();
            
        level.CreateRoom(new RoomDescription
        {
            Geometry = new Polygon([
                new LineString([
                    new Position(0.1, .1),
                    new Position(.1, -.1),
                    new Position(-.1, -.1),
                    new Position(-.1, .1),
                    new Position(.1, .1),
                ])
            ]),
            Type = Randomizer.Enum<RoomType>(),
            ArchitectualId = Guid.NewGuid().ToString(),
        });

        level.CreateWall(new LineString([
            new Position(0.5, 0.5),
            new Position(0.7, 0.7),
        ]));

        return building;
    }
}