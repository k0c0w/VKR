using System.Runtime.CompilerServices;
using HtmlAgilityPack;
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

    protected static async ValueTask<HtmlDocument> LoadContentAsHtmlDocumentAsync(HttpContent httpContent, CancellationToken ct = default)
    {
        var html = await httpContent.ReadAsStringAsync(ct); 

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc;
    }

    protected void LogError(Exception ex, [CallerMemberName] string? calledFromMethod = default)
    {
        Logger.LogError(ex, "Failed to fetch in '{calledFromMethod}': {message}", calledFromMethod, ex.Message);
    }
}