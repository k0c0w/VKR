using Domain.Errors;
using Microsoft.Extensions.Options;
using ResultMonad;
using Services.Implementation.OSM;
using Services.Map;
using UseCases;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Endpoints.Map;

namespace WebApi;

internal static class ServiceRegistry
{
    public static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.AddHttpClient();
        builder.Services.AddLogging(cfg => cfg.AddConsole());
        
        builder.Services.AddFusionCache()
            .WithSystemTextJsonSerializer();
        
        AddDomainServices(builder.Services, builder.Configuration);
        AddUseCases(builder.Services);
        AddValidators(builder.Services);
        
        return builder.Build();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddSingleton<RetrieveBuildingByAddressDtoValidator>();
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
        services.AddScoped<IUseCase<RetrieveBuildingByAddressDto, Result<BuildingDto, ErrorMessage>>, RetrieveBuildingByAddressUseCase>();
    }
}