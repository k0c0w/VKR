using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Services.EKsu;
using Services.Implementation.EKsu.Authorization;
using Services.Implementation.EKsu.BuildingCatalogue;
using Services.Implementation.EKsu.ItEquipmentCatalogue;

namespace Services.Implementation.EKsu;

public static class ServiceCollectionExtensions
{
    public static void AddEKsuServices(this IServiceCollection services)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        services.AddHttpClient(EKsuClientBase.ClientName, client =>
        {
            client.BaseAddress = new Uri("https://portal-dis.kpfu.ru");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DisKfuInteractivePlansApp");
        });

        services.AddScoped<EKsuAuthorizationClient>();
        services.AddScoped<IBuildingCatalogue, BuildingCatalogueClient>()
            .Decorate<IBuildingCatalogue, BuildingCatalogueClientCacheDecorator>();
        services.AddScoped<IItEquipmentCatalogue, ItEquipmentCatalogueClient>()
            .Decorate<IItEquipmentCatalogue, ItEquipmentCatalogueClientCacheDecorator>();
    }
}