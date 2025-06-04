using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace Services.Implementation.DisKfu;

public abstract class DisKfuClientBase
{
    public const string ClientName = "DisKfuClient";
    
    protected ILogger Logger { get; }

    protected HttpClient HttpClient { get; }

    public DisKfuClientBase(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        Logger = logger;
        HttpClient = httpClientFactory.CreateClient(ClientName);
    }

    protected static async ValueTask<IDocument> LoadContentAsHtmlDocumentAsync(HttpContent httpContent, CancellationToken ct = default)
    {
        var html = await httpContent.ReadAsStringAsync(ct); 

        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html), ct);
    }

    protected void LogError(Exception ex, [CallerMemberName] string? calledFromMethod = default)
    {
        Logger.LogError(ex, "Failed to fetch in '{calledFromMethod}': {message}", calledFromMethod, ex.Message);
    }
    
    protected static bool ContainsSessionExpiredScript(IDocument document)
    {
        const string sessionExpiredMarker = "alert(\"Извините, устарела сессия работы с системой. Пройдите процедуру авторизации.\");";

        var scriptNodes = document.QuerySelectorAll("script");
        return scriptNodes.Any(script => 
            script.TextContent.Contains(sessionExpiredMarker, StringComparison.OrdinalIgnoreCase));
    }

    protected Uri ToAbsoluteUrl(string relativeUri) => new (HttpClient.BaseAddress, relativeUri);
}