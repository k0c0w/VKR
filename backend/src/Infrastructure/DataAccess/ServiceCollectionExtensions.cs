using DataAccess.Abstractions;
using DataAccess.Repositories;
using Domain;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder
            .UseJsonNet();

        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);

        services.AddScoped<IUnitOfWork, SystemTransactionUnitOfWork>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        
        return services;
    }
}