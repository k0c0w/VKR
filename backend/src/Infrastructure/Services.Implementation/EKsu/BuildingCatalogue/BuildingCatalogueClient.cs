using System.Diagnostics.CodeAnalysis;
using System.Net;
using AngleSharp.Dom;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ResultMonad;
using System.Text.RegularExpressions;
using Common;
using Services.EKsu;
using Services.EKsu.BuildingCatalogue;
using Services.Implementation.Utils;

namespace Services.Implementation.EKsu.BuildingCatalogue;

public class BuildingCatalogueClient(
    IHttpClientFactory clientFactory,
    ILogger<IBuildingCatalogue>? logger = default)
    : EKsuClientBase(clientFactory, logger ?? NullLogger<IBuildingCatalogue>.Instance), IBuildingCatalogue
{
    private static volatile string[]? CachedRegions = default;

    public async Task<Result<string[], ErrorMessage>> GetRegionsAsync(CancellationToken ct)
    {
        var query = ConstructRegionFetchQuery();
        var response = await FetchRoomListAsync(query, ct);
        if (response.IsFailure)
        {
            return Result.Fail<string[], ErrorMessage>(response.Error);
        }

        var html = response.Value!;
        var select = html.QuerySelector("select[name='p_place']");
        if (select == null)
        {
            Logger.LogWarning("No <select name='p_place'> found in the document.");
            return Result.Fail<string[], ErrorMessage>(ErrorMessage.DomainError("Не удалось получить список регионов."));
        }

        var regions = select.QuerySelectorAll("option")
                           .Select(opt => opt.Attributes.FirstOrDefault(x => x.Name == "value"))
                           .Where(attr => attr is not null)
                           .Cast<IAttr>()
                           .Select(attr => attr.Value)
                           .Where(text => !string.IsNullOrEmpty(text))
                           .Distinct()
                           .ToArray();

        CachedRegions = regions;
        
        return Result.Ok<string[], ErrorMessage>(regions.ToArray());
    }

    public async Task<Result<Building[], ErrorMessage>> GetBuildingsAsync(string region, CancellationToken ct)
    {
        if (CachedRegions is not null && !CachedRegions.Contains(region))
        {
            return Result.Fail<Building[], ErrorMessage>(ErrorMessage.ValidationError("Неизвестный регион."));
        }

        var selectBuildingsQuery = ConstructBuildingsFetchQuery(region);
        var response = await FetchRoomListAsync(selectBuildingsQuery, ct);
        if (response.IsFailure)
        {
            return Result.Fail<Building[], ErrorMessage>(response.Error);
        }

        var html = response.Value!;
        try
        {
            var buildings = ParseBuildings(html);
            return Result.Ok<Building[], ErrorMessage>(buildings);
        }
        catch (FormatException ex)
        {
            Logger.LogCritical(ex, "The format of room id was not int64 with region {Region}.", region);
            return Result.Fail<Building[], ErrorMessage>(ErrorMessage.SystemError("Получен не верный тип id комнаты."));
        }
    }

    public async Task<Result<Building, ErrorMessage>> GetBuildingAsync(string region, string name, string address, CancellationToken ct)
    {
        if (CachedRegions is not null && !CachedRegions.Contains(region))
        {
            return Result.Fail<Building, ErrorMessage>(ErrorMessage.ValidationError("Неизвестный регион."));
        }

        var buildingsByRegionQuery = ConstructBuildingsFetchQuery(region);
        var response = await FetchRoomListAsync(buildingsByRegionQuery, ct);
        if (response.IsFailure)
        {
            return Result.Fail<Building, ErrorMessage>(response.Error);
        }

        var html = response.Value!; 
        try
        {
            var buildings = ParseBuildings(html);
            var building = buildings.FirstOrDefault(b =>
                b.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                b.Address.Equals(address, StringComparison.OrdinalIgnoreCase));

            return building == null 
                ? Result.Fail<Building, ErrorMessage>(ErrorMessage.EntityNotfoundError) 
                : Result.Ok<Building, ErrorMessage>(building);
        }
        catch (FormatException ex)
        {
            Logger.LogCritical(ex, "The format of room id was not int64 with region {region}.", region);
            return Result.Fail<Building, ErrorMessage>(ErrorMessage.SystemError("Получен не верный тип id комнаты."));
        }
    }
    
    private async Task<Result<IDocument, ErrorMessage>> FetchRoomListAsync([StringSyntax("a=1&b=example")] string selectQuery, CancellationToken ct)
    {
        const string endpoint = "e-ksu/ias_utils.treference_place";

        try
        {
            var response = await HttpClient.GetAsync($"{endpoint}?{selectQuery}", ct);
            if (response is { IsSuccessStatusCode: true, Content.Headers.ContentType.MediaType: "text/html" })
            {
                var document = await LoadContentAsHtmlDocumentAsync(response.Content, ct);
                return Result.Ok<IDocument, ErrorMessage>(document);
            }

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.NotFound })
            {
                return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.EntityNotfoundError);
            }

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.Forbidden }
                or { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.Unauthorized })
            {
                Logger.LogWarning("Forbidden access to {Endpoint}", endpoint);
                return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.AuthenticationErrors.Unauthorized);
            }

            if (!response.IsSuccessStatusCode && (int)response.StatusCode / 100 == 5)
            {
                Logger.LogError("Upstream server of {Endpoint} returned Server Error 500, roomId", endpoint);
            }

            return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.AbstractError);
        }
        catch (HttpRequestException ex)
        {
            LogError(ex);
            return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.SystemError("Не удалось выполнить запрос к каталогу помещений."));
        }
    }

    private static bool IsPureDiv(IElement element)
    {
        return element is { NodeName: "DIV", Attributes.Length: 0, ClassList.Length: 0 };
    }

    private static bool IsChapterDiv(IElement element)
    {
        if (element.NodeName != "DIV")
        {
            return false;
        }

        var idAttr = element.Attributes.GetNamedItem("id");
        return idAttr is not null
               && idAttr.Value.StartsWith("chapter");
    }

    private static Building[] ParseBuildings(IDocument document)
    {
        const string buildingsContainingElementSelector = "div.formtable > fieldset";
        var fieldset = document.QuerySelector(buildingsContainingElementSelector);
        if (fieldset == null)
        {
            return [];
        }

        var buildings = new List<Building>();
        foreach(var element in fieldset.Children)
        {
            if (!IsPureDiv(element))
            {
                continue;
            }
            
            var (buildingName, buildingAddress) = ExtractBuildingInfo(element);
            var levels = element.NextElementSibling is not null && IsChapterDiv(element.NextElementSibling) 
                ? ExtractLevels(element.NextElementSibling)
                : [];

            buildings.Add(new Building
            {
                Name = buildingName,
                Address = buildingAddress,
                Levels = levels
            });
        }

        return buildings.ToArray();
    }
        
    private static (string Name, string Address) ExtractBuildingInfo(IElement buildingInfoDiv)
    {
        var name = ExtractSecondLinkText(buildingInfoDiv);
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"Building name was empty: {buildingInfoDiv.InnerHtml}.");
        }

        var fontTag = buildingInfoDiv.QuerySelector("font");
        if (fontTag is null)
        {
            throw new ArgumentException($"Address of building {name} was not found: {buildingInfoDiv.InnerHtml}.");
        }
        
        var rawAddress = fontTag.TextContent.Trim();
        
        var indexOfSemiColumn = rawAddress.IndexOf(';');
        if (indexOfSemiColumn == -1)
        {
            var addressWithoutBrackets = rawAddress.Substring(1, rawAddress.Length - 2);
            return (name, addressWithoutBrackets);
        }
        
        var spaceAfterSemiColumnsSkipped = indexOfSemiColumn + 2;
        var indexOfClosingBracket = rawAddress.Length - 1;
        var actualAddressLenght = indexOfClosingBracket - spaceAfterSemiColumnsSkipped;
        var address = rawAddress.Substring(spaceAfterSemiColumnsSkipped, actualAddressLenght);

        return (name, address);
    }

    private static List<Level> ExtractLevels(IElement levelsContainingDiv)
    {
        if (!levelsContainingDiv.HasChildNodes)
        {
            return [];
        }

        var levels = new List<Level>();
        foreach (var element in levelsContainingDiv.Children)
        {
            if (!IsPureDiv(element))
            {
                continue;
            }

            var levelName = ExtractSecondLinkText(element);
            if (string.IsNullOrEmpty(levelName))
            {
                throw new ArgumentException($"Level name was empty: {levelsContainingDiv.InnerHtml}.");
            }
            var rooms = element.NextElementSibling is not null && IsChapterDiv( element.NextElementSibling) 
                ? ExtractRooms(element.NextElementSibling)
                : [];
            
            levels.Add(new Level
            {
                Name = levelName,
                Rooms = rooms
            });
        }

        return levels;
    }

    private static List<Room> ExtractRooms(IElement roomsContainingDiv)
    {
        if (!roomsContainingDiv.HasChildNodes)
        {
            return [];
        }

        return roomsContainingDiv.Children
            .Where(IsPureDiv)
            .Select(ExtractRoom)
            .Where(r => r is not null)
            .Cast<Room>()
            .OrderBy(x => x.RoomId)
            .ThenBy(x => x.Name)
            .ToList();
    }

    private static Room? ExtractRoom(IElement roomContainingDiv)
    {
        var link = roomContainingDiv.QuerySelector("a[onclick^='javascript:insert_list']");
        if (link == null)
        {
            return null;
        }

        var onclick = link.GetAttribute("onclick");
        if (string.IsNullOrEmpty(onclick))
        {
            return null;
        }
        
        var idMatch = Regex.Match(onclick, @"insert_list\('(\d+)',");
        var roomId = idMatch.Success ? idMatch.Groups[1].Value : null;

        var roomName = link.TextContent.Trim();
        return string.IsNullOrEmpty(roomName) 
            ? null 
            : new Room { RoomId = long.Parse(roomId), Name = roomName };
    }
    
    private static string ExtractSecondLinkText(IElement element)
    {
        var anchorTags = element.QuerySelectorAll("a");
        if (anchorTags.Length < 2)
        {
            throw new ArgumentException($"At least 2 <a> tags might be in {element.InnerHtml}.", nameof(element));
        }

        return anchorTags[1].TextContent.Trim();
    }

    private const long ShipBuildingFunctionOptionValue = 536;
    private static string ConstructRegionFetchQuery() => ConstructFormFilterQuery(null, ShipBuildingFunctionOptionValue);

    private static string ConstructBuildingsFetchQuery(string region) => ConstructFormFilterQuery(region, null);
    
    private static string ConstructFormFilterQuery(string? region, long? buildingFunction)
    {
        const string defaultQueryParams = "p_listname=p_place&p_listcount=1";
        using var valueStringBuilder = new ValueStringBuilder();
        valueStringBuilder.Append(defaultQueryParams);

        if (!string.IsNullOrEmpty(region))
        {
            valueStringBuilder.Append("&p_name=");
            valueStringBuilder.Append(CustomHttpUtility.UrlEncode(region));
        }
        
        valueStringBuilder.Append("&p_building_function=");
        
        if (buildingFunction.HasValue)
        {
            valueStringBuilder.Append(buildingFunction.Value.ToString());
        }

        return valueStringBuilder.ToString();
    }
}