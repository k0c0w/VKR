using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using Domain;
using Domain.Errors;
using Domain.GeoJson;
using ResultMonad;
using Services.Implementation.OSM.Models;
using Services.Map;
using Microsoft.Extensions.Logging;

namespace Services.Implementation.OSM;

public class OverpassApiClient : IMapProviderService
{
    private const string InterpreterEndpoint = "/api/interpreter";
    
    private readonly SemaphoreSlim _semaphore = new (1, 1);
    
    private string OverpassApiHost { get; }
    
    private HttpClient Http { get; }
    
    private ILogger<OverpassApiClient>? Logger { get; }
    
    public OverpassApiClient(
        [StringSyntax(StringSyntaxAttribute.Uri)] string host, 
        HttpClient client,
        ILogger<OverpassApiClient>? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(host, nameof(host));
        
        OverpassApiHost = host;
        Http = client ?? throw new ArgumentNullException(nameof(client));
        Logger = logger;
    }
    
    public async Task<Result<BuildingInformation, ErrorMessage>> GetBuildingInformationAsync(Address address,
        CancellationToken ct)
    {
        var uriBuilder = new UriBuilder($"{OverpassApiHost}{InterpreterEndpoint}")
        {
            Port = -1,
        };
        var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
        queryParams["data"] = ConstructBuildingFetchQuery(address);
        uriBuilder.Query = queryParams.ToString();
        
        await _semaphore.WaitAsync(ct);
        try
        {
            var response = await Http.GetAsync(uriBuilder.ToString(), ct);

            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != "application/json")
            {
                Logger?.LogInformation(
                    "Expected Content-Type 'application/json', but received {contentType} in {methodName}.",
                    contentType, nameof(GetBuildingInformationAsync));

                return Result.Fail<BuildingInformation, ErrorMessage>(MapProviderErrors.GlobalError);
            }

            var payload = await response.Content.ReadFromJsonAsync<OverpassApiResponseJsonModel>(ct);
            if (payload is null)
            {
                return Result.Fail<BuildingInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            // todo: assume only one way for building
            var targetWay = payload.Elements.FirstOrDefault(x => x.Type == "way");
            if (targetWay is null)
            {
                return Result.Fail<BuildingInformation, ErrorMessage>(MapProviderErrors.BuildingNotFoundError);
            }

            var geometry = new[]
            {
                targetWay.Geometry
                    .Select(latLng => new LatLng { Lat = latLng.Lat, Lng = latLng.Lng })
                    .ToArray()
            };

            return Result.Ok<BuildingInformation, ErrorMessage>(new BuildingInformation
            {
                Address = address,
                LevelsCount = targetWay.Tags.LevelCount.HasValue ? (uint)targetWay.Tags.LevelCount.Value : 1,
                Geometry = new BuildingGeometry(geometry),
            });
        }
        catch (HttpRequestException ex)
        {
            Logger?.LogError(ex, "HttpRequestException in {methodName}: {message}.", nameof(GetBuildingInformationAsync), ex.Message);

            var error = ex.StatusCode == HttpStatusCode.TooManyRequests
                ? MapProviderErrors.RateLimitError
                : MapProviderErrors.GlobalError;
            return Result.Fail<BuildingInformation, ErrorMessage>(error);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private string ConstructBuildingFetchQuery(Address address)
    {
        using var sb = new ValueStringBuilder();
        foreach (var letter in address.StreetName)
        {
            if (letter == 'ё' || letter == 'е')
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
    
    private ref struct ValueStringBuilder
    {
        private int _bufferPosition;
        private Span<char> _buffer;
        private char[]? _arrayFromPool;

        public ValueStringBuilder()
        {
            _bufferPosition = 0;
            _buffer = new char[32];
            _arrayFromPool = null;
        }

        public void Append(char c)
        {
            if (_bufferPosition == _buffer.Length - 1)
            {
                Grow();
            }

            _buffer[_bufferPosition++] = c;
        }

        public void Append(ReadOnlySpan<char> str)
        {
            var newSize = str.Length + _bufferPosition;
            if (newSize > _buffer.Length)
                Grow(newSize * 2);

            str.CopyTo(_buffer[_bufferPosition..]);
            _bufferPosition += str.Length;
        }

        public override string ToString() => new(_buffer[.._bufferPosition]);

        public void Dispose()
        {
            if (_arrayFromPool is not null)
            {
                ArrayPool<char>.Shared.Return(_arrayFromPool);
            }
        }

        private void Grow(int capacity = 0)
        {
            var currentSize = _buffer.Length;
            var newSize = capacity > 0 ? capacity : currentSize * 2;
            var rented = ArrayPool<char>.Shared.Rent(newSize);
            var oldBuffer = _arrayFromPool;
            _buffer.CopyTo(rented);
            _buffer = _arrayFromPool = rented;
            if (oldBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(oldBuffer);
            }
        }
    }
}