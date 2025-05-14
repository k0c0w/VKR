using DataAccess;
using Domain.Errors;
using FluentValidation;
using Microsoft.Extensions.Options;
using Migrations;
using ResultMonad;
using Services;
using Services.Implementation.OSM;
using Services.Map;
using UseCases;
using UseCases.Plans;
using UseCases.Plans.Models;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Common.Validation;
using WebApi.Endpoints.Plans;
using WebApi.Utils;

namespace WebApi;

internal static class ServiceRegistry
{
    public static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.AddHttpClient();
        builder.Services.AddLogging(cfg => cfg.AddConsole());
        
        AddCache(builder.Services);
        
        AddDatabase(builder.Services, builder.Configuration);
        
        AddDomainServices(builder.Services, builder.Configuration);
        AddUseCases(builder.Services);
        AddValidators(builder.Services);
        
        return builder.Build();
    }

    private static void AddCache(IServiceCollection services)
    {
        services.AddFusionCache()
            .WithSystemTextJsonSerializer();
    }
    
    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var appDbConnectionString = configuration.GetConnectionString("Default");
        ArgumentException.ThrowIfNullOrEmpty(appDbConnectionString, nameof(appDbConnectionString));

        var migrationConnectionString = configuration.GetConnectionString("Migrations");
        ArgumentException.ThrowIfNullOrEmpty(migrationConnectionString, nameof(migrationConnectionString));
        services.AddMigrator(migrationConnectionString);

        services.AddDataAccess(appDbConnectionString);
    }
    
    private static void AddValidators(IServiceCollection services)
    {
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) 
            => member is not null ? PropertyNameConverter.SnakeCase(member.Name) : default;
        
        services.AddSingleton<AddressDtoValidator>();
        services.AddSingleton<AddressDtoValidator>();
    }
    
    private static void AddDomainServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAddressParser, AddressParserImpl>();
        
        services.Configure<MapServiceConfiguration>(configuration.GetRequiredSection("OverpassApi"))
            .AddSingleton<IMapProviderService, OverpassApiClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var overpassApiHost = sp.GetRequiredService<IOptions<MapServiceConfiguration>>().Value.OverpassApiHost;
                    
                return new OverpassApiClient(overpassApiHost, httpClient);
            })
            .Decorate<IMapProviderService, MapProviderServiceCacheDecorator>();
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<IUseCase<RetrieveBuildingByAddressArgs, Result<BuildingDto, ErrorMessage>>, RetrieveBuildingByAddressUseCase>();
        services.AddScoped<IUseCase<GetPlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>>, GetPlanUseCase>();
        services.AddScoped<IUseCase<Result<BuildingPlanShortcut[], ErrorMessage>>, GetAvailablePlansListUseCase>();
    }
}