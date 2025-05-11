using DataAccess;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Migrations;
using Npgsql;

namespace IntegrationTests.DatabaseTests;

[Collection(nameof(DatabaseTestsCollection))]
public abstract class DbTestsBase : IDisposable
{
    private static SemaphoreSlim InitSemaphore { get; } = new (1, int.MaxValue);
    private static int _runningInstanceCount;
    private static IServiceProvider? serviceProvider;
    private static string testDbName;
    private static string baseConnectionString;
    protected static IServiceProvider ServiceProvider => serviceProvider ?? throw new InvalidOperationException($"{nameof(ServiceProvider)} is not ready yet.");

    static DbTestsBase()
    {
        InitSemaphore.Wait();
        if (Interlocked.Increment(ref _runningInstanceCount) == 1)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("tests_settings.json", optional: false, reloadOnChange: false)
                .Build();

            var conf = configuration.GetSection("TestDatabase");
            testDbName = conf.GetValue<string>("DatabaseName") ?? "";
            baseConnectionString = conf.GetValue<string>("MasterConnectionString") ?? "";
            var connectionString = $"{baseConnectionString};Database={testDbName};";

            ArgumentException.ThrowIfNullOrEmpty(testDbName);
            ArgumentException.ThrowIfNullOrEmpty(baseConnectionString);
            
            DropTestDatabase();
            CreateTestDatabase();
            RunMigrations(connectionString);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataAccess(connectionString);
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        InitSemaphore.Release(int.MaxValue);
    }

    private static void CreateTestDatabase()
    {
        using var conn = new NpgsqlConnection(baseConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand($"CREATE DATABASE {testDbName};", conn);
        cmd.ExecuteNonQuery();
    }

    private static void RunMigrations(string connectionString)
    {
        using var newServiceProvider = new ServiceCollection()
            .AddMigrator(connectionString)
            .BuildServiceProvider();

        var runner = newServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private static void DropTestDatabase()
    {
        using var conn = new NpgsqlConnection(baseConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {testDbName} WITH (FORCE);", conn);
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _runningInstanceCount) == 0)
        {
            DropTestDatabase();
        }
    }
}