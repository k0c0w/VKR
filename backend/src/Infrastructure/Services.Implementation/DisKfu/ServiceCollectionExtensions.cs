using System.Text;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Services.DisKfuAuthorization;
using Services.Implementation.DisKfu.Authorization;
using Services.ItEquipmentCatalogue;

namespace Services.Implementation.DisKfu;

public static class ServiceCollectionExtensions
{
    public static void AddDisKfuServices(this IServiceCollection services)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        services.AddHttpClient(DisKfuClientBase.ClientName, client =>
        {
            client.BaseAddress = new Uri("https://portal-dis.kpfu.ru");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DisKfuInteractivePlansApp");
        });

        services.AddScoped<IDisKfuAuthorizationService, AuthorizationClient>();
        services.AddScoped<IItEquipmentCatalogue, ItEquipmentCatalogueClient>();
    }
}