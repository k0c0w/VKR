using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using Common;
using Domain.Errors;
using Domain.ValueObjects;
using GeoJSON.Net.Geometry;
using ResultMonad;
using Services.Implementation.OSM.Models;
using Services.Map;
using Microsoft.Extensions.Logging;

namespace Services.Implementation.OSM;

public class OverpassApiClient(
    IHttpClientFactory clientFactory,
    ILogger<OverpassApiClient>? logger = null)
    : IMapProviderService
{
    public const string ClientName = nameof(OverpassApiClient);
    
    private const string InterpreterEndpoint = "/api/interpreter";
    
    private readonly SemaphoreSlim _semaphore = new (1, 1);
    
    private HttpClient Http { get; } = clientFactory.CreateClient(ClientName);
    private ILogger<OverpassApiClient>? Logger { get; } = logger;

    public async Task<Result<BuildingBasementInformation, ErrorMessage>> GetBuildingInformationAsync(Address address,
        CancellationToken ct)
    {
        var overpassQuery = ConstructBuildingFetchOverpassQuery(address);
        var endpoint = $"{InterpreterEndpoint}?data={HttpUtility.UrlEncode(overpassQuery)}";
        
        await _semaphore.WaitAsync(ct);
        try
        {
            var response = await Http.GetAsync(endpoint, ct);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != "application/json")
            {
                Logger?.LogInformation(
                    "Expected Content-Type 'application/json', but received {contentType} in {methodName}.",
                    contentType, nameof(GetBuildingInformationAsync));

                return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.GlobalError);
            }

            var payload = await response.Content.ReadFromJsonAsync<OverpassApiResponseJsonModel>(ct);
            if (payload is null)
            {
                return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            // todo: assume only one way for building
            var targetWay = payload.Elements.FirstOrDefault(x => x.Type == "way");
            if (targetWay is null)
            {
                return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            var geometry = new[]
            {
                new LineString(targetWay.Geometry
                    .Select(latLng => new Position(latitude:latLng.Lat, longitude:latLng.Lng)))
            };

            return Result.Ok<BuildingBasementInformation, ErrorMessage>(new BuildingBasementInformation
            {
                Address = address,
                LevelsCount = targetWay.Tags.LevelCount.HasValue ? (uint)targetWay.Tags.LevelCount.Value : 1,
                Geometry = new Polygon(geometry),
            });
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "HttpRequestException in {methodName}: {message}.", nameof(GetBuildingInformationAsync), ex.Message);

            var error = ex.StatusCode == HttpStatusCode.TooManyRequests
                ? MapProviderErrors.RateLimitError
                : MapProviderErrors.GlobalError;
            return Result.Fail<BuildingBasementInformation, ErrorMessage>(error);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private string ConstructBuildingFetchOverpassQuery(Address address)
    {
        using var sb = new ValueStringBuilder();
        foreach (var letter in address.StreetName)
        {
            if (letter is 'ё' or 'е')
            {
                sb.Append("(е|ё)");
            }
            else
            {
                sb.Append(letter);
            }
        }
        var streetName = sb.ToString();
        
        var query = 
            $"""
            [out:json];
            area[place=city][name="{address.City}"]->.searchCityArea;
            way[building]["addr:street"~"^({streetName} {address.StreetType}|{address.StreetType} {streetName})$", i]["addr:housenumber"~"^({address.HouseNumber}[[:space:]]*{address.HouseUnit})$", i](area.searchCityArea);
            out body geom;
            """;

        Logger?.LogDebug("Constructed OverpassApi query: {query}", query);
        
        return query;
    }
}