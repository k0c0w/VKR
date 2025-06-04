using Compunet.YoloSharp;
using DataAccess;
using DataAccess.Abstractions;
using DataAccess.Repositories;
using FluentValidation;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Migrations;
using Newtonsoft.Json;
using Services;
using Services.Implementation.Authorization;
using Services.Implementation.DisKfu;
using Services.Implementation.OSM;
using Services.Implementation.PlanAnalyzer;
using Services.Implementation.PlanAnalyzer.Ocr;
using Services.Map;
using Services.PlanImageAnalyzer;
using UseCases.Authorization;
using UseCases.Plans;
using UseCases.RetrieveBuildingByAddress;
using WebApi.BackgroundWorkers;
using WebApi.ExceptionHandlers;
using ZiggyCreatures.Caching.Fusion;

namespace WebApi;

internal static class ServiceRegistry
{
    public static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.AddLogging(cfg => cfg.AddConsole());

        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            });
        builder.Services.AddAuthentication(AuthorizationService.AuthenticationScheme)
            .AddCookie(AuthorizationService.AuthenticationScheme, options =>
            {
                options.Cookie.Name = AuthorizationService.AuthenticationScheme;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/authorization/sign-in";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
            });
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();

        AddCache(builder.Services, builder.Configuration);
        
        AddDatabase(builder.Services, builder.Configuration);
        
        AddDomainServices(builder.Services, builder.Configuration);
        AddUseCases(builder.Services);
        AddValidators(builder.Services);

        AddPlanRecognition(builder.Services, builder.Configuration);
        
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
        services.AddValidatorsFromAssemblies([typeof(Program).Assembly]);
    }
    
    private static void AddDomainServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAddressParser, AddressParserImpl>();

        var overpassApiHost = configuration.GetRequiredSection("OverpassApi")?.Value ??
                              throw new InvalidOperationException("Provide Overpass Api host.");
        
        services.AddSingleton<IMapProviderService, OverpassApiClient>()
            .Decorate<IMapProviderService, MapProviderServiceCacheDecorator>()
            .AddHttpClient(OverpassApiClient.ClientName, client =>
            {
                client.BaseAddress = new Uri(overpassApiHost);
            });
        
        services.AddDisKfuServices();
        services.AddScoped<Domain.Services.IAuthorizationService, AuthorizationService>();
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<RetrieveBuildingByAddressUseCase>();
        services.AddScoped<GetPlanUseCase>();
        services.AddScoped<GetAvailablePlansListUseCase>();
        services.AddScoped<CreatePlanUseCase>();
        services.AddScoped<SignInUseCase>();
    }

    private static void AddPlanRecognition(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InMemoryPlanImageBus.PlanImageBusOptions>(configuration.GetSection("DataBus"));
        
        services.AddSingleton<InMemoryPlanImageBus>();
        services.AddScoped<IPublisher<PlanImageMessage>>(sp => sp.GetRequiredService<InMemoryPlanImageBus>());
        services.AddHostedService<ProcessPlanImageConsumerBackgroundService>();
        services.AddScoped<IPlanImageAnalysisRepository, PlanImageAnalysisRepository>();
        services.AddScoped<IPlanImageAnalyzerService, PlanImageAnalyzer>();
        services.AddSingleton<IOcr, NoOcr>();

        var yoloSection = configuration.GetRequiredSection("Yolo");
        var weightsPath = yoloSection.GetValue<string>("PathToWeights") ?? throw new InvalidOperationException("Provide Yolo:PathToWeights");
        var useCuda = yoloSection.GetValue<bool>("UseCuda");
        services.AddScoped<YoloPredictor>(_ => new YoloPredictor(weightsPath, 
            new YoloPredictorOptions
            {
                UseCuda = useCuda,
            })
        );
    }
}