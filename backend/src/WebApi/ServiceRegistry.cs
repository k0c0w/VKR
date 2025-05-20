using DataAccess;
using FluentValidation;
using GeoJSON.Net.Converters;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Migrations;
using Newtonsoft.Json;
using Services;
using Services.Implementation.OSM;
using Services.Map;
using UseCases.Plans;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Common.Validation;
using WebApi.Utils;
using ZiggyCreatures.Caching.Fusion;

namespace WebApi;

internal static class ServiceRegistry
{
    public static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.AddHttpClient();
        builder.Services.AddLogging(cfg => cfg.AddConsole());

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            });

        AddCache(builder.Services, builder.Configuration);
        
        AddDatabase(builder.Services, builder.Configuration);
        
        AddDomainServices(builder.Services, builder.Configuration);
        AddUseCases(builder.Services);
        AddValidators(builder.Services);
        
        return builder.Build();
    }

    private static void AddCache(IServiceCollection services, IConfiguration configuration)
    {
        var cacheBuilder = services.AddFusionCache();
        cacheBuilder.WithNewtonsoftJsonSerializer();
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (redisConnectionString is not null)
        {
            cacheBuilder.WithDistributedCache(_ =>
            {
                var options = new RedisCacheOptions { Configuration = redisConnectionString };

                return new RedisCache(options);
            });
        }
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
        
        services.AddValidatorsFromAssemblies([typeof(Program).Assembly]);
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
        services.AddScoped<RetrieveBuildingByAddressUseCase>();
        services.AddScoped<GetPlanUseCase>();
        services.AddScoped<GetAvailablePlansListUseCase>();
        services.AddScoped<CreatePlanUseCase>();
    }
}