using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using Domain.Entities;
using Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ResultMonad;
using System.Threading.Channels;
using System.Web;
using AngleSharp.Dom;
using Services.Authorization;
using Services.EKsu;
using Services.Implementation.EKsu.Authorization;

namespace Services.Implementation.EKsu.ItEquipmentCatalogue;

public class ItEquipmentCatalogueClient(
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory clientFactory,
    ILogger<IItEquipmentCatalogue>? logger = default)
    : EKsuClientBase(clientFactory, logger ?? NullLogger<IItEquipmentCatalogue>.Instance), IItEquipmentCatalogue
{
    public async Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsAsync(long[] roomIds, CancellationToken ct = default)
    {
        if (roomIds.Length == 0)
        {
            return Result.Ok<ItEquipmentDescription[], ErrorMessage>([]);
        }

        var equipmentResult = await (roomIds.Length < 3 
            ? GetAllItEquipmentByRoomIdsSequentialStrategyAsync(roomIds, ct) 
            : GetAllItEquipmentByRoomIdsParallelizationStrategyAsync(roomIds, ct));

        if (equipmentResult.IsFailure)
        {
            return equipmentResult;
        }

        return Result.Ok<ItEquipmentDescription[], ErrorMessage>(equipmentResult.Value
            .Where(x => x.LocationAudienceCatalogueId != 0)
            .DistinctBy(x => x.Id)
            .ToArray());
    }

    private async Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsSequentialStrategyAsync(long[] roomIds, CancellationToken ct)
    {
        var result = new List<ItEquipmentDescription>();
        
        foreach (var roomId in roomIds)
        {
            var fetchResult = await FetchSingleRoomEquipmentAsync(roomId, ct);
            switch (fetchResult.IsFailure)
            {
                case true when fetchResult.Error == ErrorMessage.EntityNotfoundError:
                    continue;
                case true:
                    return Result.Fail<ItEquipmentDescription[], ErrorMessage>(fetchResult.Error);
            }

            var parsed = ParseResponse(fetchResult.Value!, roomId);
            if (parsed.IsSuccess)
            {
                result.AddRange(parsed.Value!);
            }
        }

        return Result.Ok<ItEquipmentDescription[], ErrorMessage>(result.ToArray());
    }
    
    private async Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsParallelizationStrategyAsync(long[] roomIds, CancellationToken ct)
    {
        const int parallelRequestsCount = 3;
        const int parallelParsersCount = 2;
        const int totalBackgroundWorkers = parallelParsersCount + parallelParsersCount + 1;
        
        var operationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var roomIdChannel = Channel.CreateBounded<long>(new BoundedChannelOptions(5) { SingleWriter = true, SingleReader = false });
        var responseChannel = Channel.CreateBounded<(long RoomId, IDocument Html)>(new BoundedChannelOptions(5) { SingleWriter = false, SingleReader = false });
        var allEquipment = new ConcurrentBag<List<ItEquipmentDescription>>();
        var authErrorDuringRequests = default(ErrorMessage?);
        var runningRequestWorkersCount = parallelRequestsCount;
        var tryCloseRequestWorkerChannel = () =>
        {
            if (Interlocked.Decrement(ref runningRequestWorkersCount) == 0)
            {
                responseChannel.Writer.TryComplete();
            }
        };
        Action<ErrorMessage> setAuthErrorDuringRequests = err => authErrorDuringRequests = err;

        var workers = new List<Task>(totalBackgroundWorkers)
        {
            ProduceRoomsPipe(roomIdChannel.Writer, operationCts.Token),
        };
        var requestWorkers = Enumerable.Range(0, parallelRequestsCount)
            .Select(_ => MakeHttpRequestsOrEarlyStopPipe(roomIdChannel.Reader, responseChannel.Writer, 
                setAuthErrorDuringRequests, tryCloseRequestWorkerChannel, operationCts));
        workers.AddRange(requestWorkers);
        var parsersWorkers = Enumerable.Range(0, parallelParsersCount)
            .Select(_ => ParseHtmlResponsePipe(responseChannel.Reader, allEquipment, operationCts.Token));
        workers.AddRange(parsersWorkers);

        try
        {
            await Task.WhenAll(workers);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        finally
        {        
            responseChannel.Writer.TryComplete();
            roomIdChannel.Writer.TryComplete();
        }

        if (authErrorDuringRequests.HasValue)
        {
            return Result.Fail<ItEquipmentDescription[], ErrorMessage>(authErrorDuringRequests.Value!);
        }

        return Result.Ok<ItEquipmentDescription[], ErrorMessage>(allEquipment.
            SelectMany(x => x)
            .ToArray());

        async Task ProduceRoomsPipe(ChannelWriter<long> writer, CancellationToken token)
        {
            foreach (var roomId in roomIds)
            {
                await writer.WriteAsync(roomId, token);
            }
            roomIdChannel.Writer.Complete();
        }
        
        async Task MakeHttpRequestsOrEarlyStopPipe(
            ChannelReader<long> reader, 
            ChannelWriter<(long, IDocument)> writer,
            Action<ErrorMessage> setAuthError,
            Action onFinish,
            CancellationTokenSource wholePipelineCancellationTokenSource)
        {
            await Task.Yield();
            while (await reader.WaitToReadAsync(wholePipelineCancellationTokenSource.Token))
            {
                if (!roomIdChannel.Reader.TryRead(out var roomId))
                {
                    continue;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(300), ct);
                var fetchResult = await FetchSingleRoomEquipmentAsync(roomId, wholePipelineCancellationTokenSource.Token);
                if (fetchResult.IsFailure &&
                    (fetchResult.Error == ErrorMessage.AuthenticationErrors.Unauthorized ||
                     fetchResult.Error == ErrorMessage.AuthenticationErrors.AccessDenied))
                {
                    await wholePipelineCancellationTokenSource.CancelAsync();
                    setAuthError(fetchResult.Error);
                }

                if(fetchResult.IsSuccess)
                {
                    await writer.WriteAsync((roomId, fetchResult.Value!), wholePipelineCancellationTokenSource.Token);
                }
            }

            onFinish();
        }

        async Task ParseHtmlResponsePipe(ChannelReader<(long RoomId, IDocument Html)> reader, 
            ConcurrentBag<List<ItEquipmentDescription>> resultAccumulator,  
            CancellationToken token)
        {
            await Task.Yield();
            while (await reader.WaitToReadAsync(token))
            {
                if (!reader.TryRead(out var response))
                {
                    continue;
                }
                var parseResult = ParseResponse(response.Html, response.RoomId);
                if (parseResult.IsSuccess)
                {
                    resultAccumulator.Add(parseResult.Value!);
                }
                else
                {
                    Logger.LogInformation("Error during parsing, skipping: {error}", parseResult.Error.ToString());
                }
            }
        }
    }
    
    private async Task<Result<IDocument, ErrorMessage>> FetchSingleRoomEquipmentAsync(long roomId, CancellationToken ct)
    {
        const string equipmentListFormEndpoint = "e-ksu/it_equipment_pack.equipment_catalog";
        var formContent = ConstructRoomItEquipmentListFilterForm(locatedAtRoomId: roomId);
        try
        {
            var response = await HttpClient.PostAsync(equipmentListFormEndpoint, formContent, ct);

            if (response.IsSuccessStatusCode && response.Content.Headers?.ContentType?.MediaType == "text/html")
            {
                var html = await LoadContentAsHtmlDocumentAsync(response.Content, ct);
                return ContainsSessionExpiredScript(html) 
                    ? Result.Fail<IDocument, ErrorMessage>(ErrorMessage.AuthenticationErrors.Unauthorized)
                    : Result.Ok<IDocument, ErrorMessage>(html);
            }

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.Forbidden })
            {
                Logger.LogWarning("Forbidden access to {Endpoint} for room ID {RoomId}", equipmentListFormEndpoint, roomId);
                return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
            }
            
            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.NotFound })
            {
                return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.EntityNotfoundError);
            }

            if (!response.IsSuccessStatusCode && (int)response.StatusCode / 100 == 5)
            {
                Logger.LogError("Upstream server of {Endpoint} returned Server Error 500, roomId", equipmentListFormEndpoint);
            }

            return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.AbstractError);
        }
        catch (HttpRequestException ex)
        {
            LogError(ex);
            return Result.Fail<IDocument, ErrorMessage>(ErrorMessage.SystemError("Не удалось выполнить запрос к каталогу ИТ-оборудования."));
        }
    }
    
    private Result<List<ItEquipmentDescription>, ErrorMessage> ParseResponse(IDocument html, long catalogueRoomId)
    {
        try
        {
            var equipmentTableResult = SelectEquipmentTable(html);
            if (equipmentTableResult.IsFailure)
            {
                Logger.LogWarning("No equipment table found in response for room ID {RoomId}", catalogueRoomId);
                return Result.Ok<List<ItEquipmentDescription>, ErrorMessage>([]);
            }

            var rows = SelectTableContentRows(equipmentTableResult.Value!);
            var equipmentList = new List<ItEquipmentDescription>();
            var historyUrlRegex = new Regex(@"openWin\('([^']+)'\s*,\s*'[^']+'\s*,\s*\d+\s*,\s*\d+\)", RegexOptions.IgnoreCase);

            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td");
                if (cells.Length < 4)
                {
                    Logger.LogWarning("Skipping malformed row for room ID {RoomId}: insufficient columns count {count}", catalogueRoomId, cells.Length);
                    continue;
                }

                const int inventoryNumberColumnIndex = 1;
                var inventoryNode = cells[inventoryNumberColumnIndex].QuerySelector("nobr");
                var inventoryNumber = inventoryNode?.TextContent?.Trim();
                if (string.IsNullOrEmpty(inventoryNumber))
                {
                    Logger.LogWarning("Skipping row with missing inventory number for room ID {RoomId}", catalogueRoomId);
                    continue;
                }

                const int equipmentNameColumnIndex = inventoryNumberColumnIndex + 1;
                var linkNode = cells[equipmentNameColumnIndex].QuerySelector("a");
                var equipmentName = linkNode?.TextContent?.Trim();
                var relativeCardUrl = linkNode?.GetAttribute("href");
                if (string.IsNullOrEmpty(equipmentName) || string.IsNullOrEmpty(relativeCardUrl))
                {
                    Logger.LogWarning("Skipping row with missing name or card URL for room ID {RoomId}", catalogueRoomId);
                    continue;
                }
                relativeCardUrl = RemoveSessionQueryParametersFromUri(relativeCardUrl);

                const int historyUrlColumnIndex = equipmentNameColumnIndex + 1;
                var historyLinkNode = cells[historyUrlColumnIndex].QuerySelector("a");
                var onClickAttr = historyLinkNode?.GetAttribute("onClick");
                string? relativeHistoryUrl = null;
                if (!string.IsNullOrEmpty(onClickAttr))
                {
                    var match = historyUrlRegex.Match(onClickAttr);
                    if (match.Success)
                    {
                        relativeHistoryUrl = RemoveSessionQueryParametersFromUri(match.Groups[1].Value);
                    }
                    else
                    {
                        Logger.LogWarning("History URL regex failed for room ID {RoomId}, onClick: {OnClick}", catalogueRoomId, onClickAttr);
                    }
                }
                else
                {
                    Logger.LogWarning("Missing onClick attribute for history link in row for room ID {RoomId}", catalogueRoomId);
                }

                equipmentList.Add(new ItEquipmentDescription
                {
                    Id = inventoryNumber,
                    Name = equipmentName,
                    ItEquipmentCardUrl = relativeCardUrl,
                    ItEquipmentHistoryUrl = relativeHistoryUrl ?? string.Empty,
                    LocationAudienceCatalogueId = catalogueRoomId
                });
            }

            return Result.Ok<List<ItEquipmentDescription>, ErrorMessage>(equipmentList);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "Failed to parse response for room ID {RoomId}", catalogueRoomId);
            return Result.Fail<List<ItEquipmentDescription>, ErrorMessage>(
                ErrorMessage.SystemError("Ошибка при парсинге ответа от каталога ИТ-оборудования."));
        }
    }
    
    private string RemoveSessionQueryParametersFromUri(string uri)
    {
        const string eKsuControllerPath = "e-ksu/";
        const string jqueryVariablePrefix = "IAS$DB.";
        var uriSpan = uri.AsSpan();
        var queryIndex = uriSpan.IndexOf('?');

        // Split into path and query
        var pathSpan = queryIndex >= 0 ? uriSpan[..queryIndex] : uriSpan;
        var querySpan = queryIndex >= 0 ? uriSpan[(queryIndex + 1)..] : ReadOnlySpan<char>.Empty;

        var queryParams = HttpUtility.ParseQueryString(querySpan.ToString());

        queryParams.Remove("p1");
        queryParams.Remove("p2");
        
        return $"{HttpClient.BaseAddress}{eKsuControllerPath}{pathSpan.Slice(jqueryVariablePrefix.Length, pathSpan.Length - jqueryVariablePrefix.Length)}?{queryParams}";
    }
    
    private FormUrlEncodedContent ConstructRoomItEquipmentListFilterForm(long locatedAtRoomId)
    {
        var context = httpContextAccessor.HttpContext;

        var entryPage = context?.User.Claims.FirstOrDefault(x => x.Type == nameof(AuthorizationPayload.EntryPageId))
                            ?.Value ??
                        string.Empty;
        var thisUserSession =
            context?.User.Claims.FirstOrDefault(x => x.Type == nameof(AuthorizationPayload.Session))?.Value ??
            string.Empty;
        var sessionHash = context?.User.Claims.FirstOrDefault(x => x.Type == nameof(AuthorizationPayload.VerificationHash))
                              ?.Value ??
                          string.Empty;
        // values interpreted as assumptions
        const string inventoryCardFormId = "388";
        const string activeFormStatus = "10";
        const string searchFormMode = "0";

        // Подразделение: "КФУ" option
        const string searchEquipmentsInAllDepartments = "1150748";
        const string startEquipmentCost = "0";
        const string dontSplitResultToPagesMode = "1";

        const string sendAcceptedEquipment = "1";
        // Лингафонный кабинет
        const string isLanguageRoom = "1";
        const string isComputerAudience = "1";
        const string isMultimediaAudience = "1";
        const string includeOtherAudienceTypes = "1";
        
        const string submitButtonValue = " Отобрать ";

        List<KeyValuePair<string, string>> formPayload =
        [
            new("p1", entryPage),
            new("p2", thisUserSession),
            new("p_h", sessionHash),
            new("p_place", locatedAtRoomId.ToString()),

            new("p_menu", inventoryCardFormId),
            new("p_status", activeFormStatus),
            new("p_form", searchFormMode),
            new("p_office_rn", searchEquipmentsInAllDepartments),
            new("p_name", string.Empty),
            new("p_inventory", string.Empty),
            new("p_purpose_id", string.Empty),
            new("p_snomer", string.Empty),
            new("p_cost1", startEquipmentCost),
            new("p_cost2", string.Empty),
            new("p_os_condition", string.Empty),
            new("p_smetka", string.Empty),
            new("p_data1", string.Empty),
            new("p_data2", string.Empty),
            new("p_ostype", string.Empty),
            new("p_snamead", string.Empty),
            new("p_create_data1", string.Empty),
            new("p_create_data2", string.Empty),
            new("p_outdate", string.Empty),
            new("p_lastdata1", string.Empty),
            new("p_lastdata2", string.Empty),
            new("p_sn", string.Empty),
            new("p_ad", string.Empty),
            new("p_place_yes", string.Empty),
            new("p_mainf_yes", string.Empty),
            new("p_face_yes", string.Empty),
            new("p_report_yes", string.Empty),
            new("p_sostav", string.Empty),
            new("p_knop", submitButtonValue),
            new("p_spisok", dontSplitResultToPagesMode),
            new("p_prinyat", sendAcceptedEquipment),
            new("p_ling", isLanguageRoom),
            new("p_class", isComputerAudience),
            new("p_media", isMultimediaAudience),
            new("p_other", includeOtherAudienceTypes),
        ];

        return new FormUrlEncodedContent(formPayload);
    }
    
    private static Result<IElement> SelectEquipmentTable(IDocument html)
    {
        var tables = html.QuerySelectorAll("table[border='1'][align='center']");
        
        foreach (var table in tables)
        {
            var firstRow = table.QuerySelector("tr");
            var headers = firstRow?.QuerySelectorAll("td");
            if (headers != null && headers.Any(td => td.InnerHtml.Trim().Contains("Инвентарный<br>номер")))
            {
                return Result.Ok(table);
            }
        }
        
        return Result.Fail<IElement>();
    }

    private static IEnumerable<IElement> SelectTableContentRows(IElement equipmentTable)
    {
        var rows = equipmentTable.QuerySelectorAll("tr");
        if (rows.Length <= 1)
        {
            return [];
        }

        var withoutHeaderRow = rows.Skip(1);
        return withoutHeaderRow;
    }
}