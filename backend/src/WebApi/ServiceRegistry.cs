using Services.Implementation.OSM;
using Services.Map;

namespace WebApi;

internal static class ServiceRegistry
{
    public static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        builder.Services.AddHttpClient();

        AddInfrastructureServices(builder.Services, builder.Configuration);
        
        return builder.Build();
    }

    private static void AddInfrastructureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MapServiceConfiguration>(configuration.GetRequiredSection("OverpassApi"))
            .AddSingleton<IMapProviderService, OverpassApiClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var overpassApiHost = sp.GetRequiredService<MapServiceConfiguration>().OverpassApiHost;
                return new OverpassApiClient(overpassApiHost, httpClient);
            })
            .Decorate<IMapProviderService, MapProviderServiceCacheDecorator>();
    }
}