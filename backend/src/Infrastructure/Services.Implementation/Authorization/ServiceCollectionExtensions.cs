using Microsoft.Extensions.DependencyInjection;
using Services.Authorization;

namespace Services.Implementation.Authorization;

public static class ServiceCollectionExtensions
{
    public const string AuthenticationScheme = AuthorizationService.AuthenticationScheme;
    
    public static IServiceCollection AddIAuthorizationServiceImplementation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthorizationService, AuthorizationService>();
        return serviceCollection;
    }
}