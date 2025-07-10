using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common;
using Domain.Errors;
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

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private HttpClient Http { get; } = clientFactory.CreateClient(ClientName);
    private ILogger<OverpassApiClient>? Logger { get; } = logger;

    public async Task<Result<BuildingBasementInformation, ErrorMessage>> GetBuildingInformationAsync(
        Services.Address.Address address, CancellationToken ct)
    {
        var overpassQuery = ConstructBuildingFetchOverpassQuery(address);

        await _semaphore.WaitAsync(ct);
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Http.BaseAddress}{InterpreterEndpoint}")
            {
                Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", overpassQuery)]),
                Headers =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("application/json")
                    },
                    Expect =
                    {
                        new NameValueWithParametersHeaderValue("Content-Type", "application/json")
                    }
                }
            };
            var response = await Http.SendAsync(request, ct);
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
            if (payload is null || !payload.Elements.Any())
            {
                return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            var result = ExtractOuterPolygon(payload.Elements);
            if (result.IsFailure)
            {
                return Result.Fail<BuildingBasementInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            var geometry = result.Value;
            return Result.Ok<BuildingBasementInformation, ErrorMessage>(new BuildingBasementInformation
            {
                Geometry = geometry,
            });
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "HttpRequestException in {methodName}: {message}.",
                nameof(GetBuildingInformationAsync), ex.Message);
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

    private Result<Polygon, ErrorMessage> ExtractOuterPolygon(ElementsJsonModel[] elements)
    {
        var building = elements.FirstOrDefault(e => e.Tags.ContainsKey("building")
                                                    && e is { Type: "way", Geometry: not null } or
                                                        { Type: "relation", Members: not null });
        if (building is null)
        {
            return Result.Fail<Polygon, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
        }

        Position[] outerRing;
        switch (building.Type)
        {
            case "way" when building.Geometry != null:
            {
                outerRing = building.Geometry.Select(latLng => new Position(latLng.Lat, latLng.Lon)).ToArray();
                if (outerRing.Length < 3 || outerRing.First() != outerRing.Last())
                {
                    return Result.Fail<Polygon, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
                }

                break;
            }
            case "relation" when building.Members != null:
            {
                var outerMember = building.Members
                    .Where(m => m.Type == "way" && m.Role == "outer" && m.Geometry != null && m.Geometry.Length >= 3)
                    .OrderByDescending(m => m.Geometry.Length)
                    .FirstOrDefault();


                if (outerMember == null)
                {
                    return Result.Fail<Polygon, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
                }

                if (outerMember.Geometry![0] != outerMember.Geometry[^1])
                {
                    outerRing = outerMember.Geometry
                        .Concat([outerMember.Geometry[0]])
                        .Select(latLng => new Position(latLng.Lat, latLng.Lon))
                        .ToArray();
                }
                else
                {
                    outerRing = outerMember.Geometry.Select(latLng => new Position(latLng.Lat, latLng.Lon)).ToArray();
                }

                break;
            }
            default:
                return Result.Fail<Polygon, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
        }

        try
        {
            var poly = new Polygon([new LineString(outerRing)]);
            return Result.Ok<Polygon, ErrorMessage>(poly);
        }
        catch
        {
            return Result.Fail<Polygon, ErrorMessage>(
                ErrorMessage.DomainError("Не удалось распрасить геометрию объекта."));
        }
    }

    private string ConstructBuildingFetchOverpassQuery(Services.Address.Address address)
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
             area[place={GetPlace(address.StreetType)}][name="{address.SettlementName}"]->.searchCityArea;
             wr[building]["addr:street"~"^({streetName} {address.StreetType}|{address.StreetType} {streetName})$", i]["addr:housenumber"~"({address.HouseNumber}[[:space:]]*{address.HouseUnit})", i](area.searchCityArea);
             out body geom;
             """;

        Logger?.LogDebug("Constructed OverpassApi query: {query}", query);

        return query;
    }

    private static string GetPlace(string settlementType)
    {
        return settlementType switch
        {
            "город" => "city",
            "поселок городского типа" => "town",
            "рабочий поселок" => "town",
            "курортный поселок" => "town",
            "городской поселок" => "town",
            "поселок" => "hamlet",
            "аал" => "village",
            "арбан" => "village",
            "аул" => "village",
            "выселки" => "hamlet",
            "городок" => "town",
            "заимка" => "locality",
            "починок" => "hamlet",
            "кишлак" => "village",
            "поселок при станции" => "hamlet",
            "поселок при железнодорожной станции" => "hamlet",
            "железнодорожный блокпост" => "locality",
            "железнодорожная будка" => "locality",
            "железнодорожная ветка" => "locality",
            "железнодорожная казарма" => "locality",
            "железнодорожный комбинат" => "locality",
            "железнодорожная платформа" => "locality",
            "железнодорожная площадка" => "locality",
            "железнодорожный путевой пост" => "locality",
            "железнодорожный остановочный пункт" => "locality",
            "железнодорожный разъезд" => "locality",
            "железнодорожная станция" => "locality",
            "местечко" => "village",
            "деревня" => "village",
            "село" => "village",
            "слобода" => "village",
            "станция" => "locality",
            "станица" => "village",
            "улус" => "village",
            "хутор" => "hamlet",
            "разъезд" => "locality",
            "зимовье" => "locality",
            _ => "city" // Fallback
        };
    }
}