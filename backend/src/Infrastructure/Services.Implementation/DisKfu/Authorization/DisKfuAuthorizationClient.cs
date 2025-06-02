using Domain.Errors;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ResultMonad;
using Services.DisKfuAuthorization;
using OutParsing;

namespace Services.Implementation.DisKfu.Authorization;

public class DisKfuAuthorizationClient : DisKfuClientBase
{
    public DisKfuAuthorizationClient(IHttpClientFactory httpClientFactory, ILogger<DisKfuAuthorizationClient>? logger = default) 
        : base(httpClientFactory, logger ?? NullLogger<DisKfuAuthorizationClient>.Instance)
    {
    }

    public async Task<Result<AuthorizationCredentials, ErrorMessage>> SignInAsync(string login, string password, CancellationToken ct)
    {
        const string authorizationEndpoint = "e-ksu/private_office.kfuscript";
        ArgumentException.ThrowIfNullOrEmpty(login, nameof(login));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

        var httpRequestPayload = new FormUrlEncodedContent([
            new ("p_login", login),
            new ("p_pass", password)
        ]);
        try
        {
            var response = await HttpClient.PostAsync(authorizationEndpoint, httpRequestPayload, ct);
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content;
            
            if (responseContent.Headers.ContentType?.MediaType == "text/html")
            {
                var htmlDocument = await LoadContentAsHtmlDocumentAsync(response.Content, ct);
                var credits = ParseResponseContent(htmlDocument);
    
                if (credits.HasValue)
                {
                    return Result.Ok<AuthorizationCredentials, ErrorMessage>(credits.Value);
                }
            }
            else
            {
                var ex = new InvalidOperationException($"Unexpected response Content-Type: {response.Content.Headers.ContentType?.MediaType}");
                LogError(ex);
            }
        }
        catch (HttpRequestException ex)
        {
            LogError(ex);
            return Result.Fail<AuthorizationCredentials, ErrorMessage>(ErrorMessage.SystemError("Не удалось выполнить запрос авторизации."));
        }
        
        return Result.Fail<AuthorizationCredentials, ErrorMessage>(ErrorMessage.AuthenticationErrors.Unauthorized);
    }

    private static AuthorizationCredentials? ParseResponseContent(HtmlDocument response)
    {
        var scriptNodes = response.DocumentNode.SelectNodes("//script");

        if (scriptNodes == null || !scriptNodes.Any())
        {
            return default;
        }

        var firstScriptNode = scriptNodes.First();
        if (string.IsNullOrEmpty(firstScriptNode.InnerText) || firstScriptNode.InnerText.StartsWith("alert('Извините, неверно введены имя или пароль');"))
        {
            return default;
        }
        
        var lastScriptNode = scriptNodes.Last();
        if (string.IsNullOrEmpty(lastScriptNode.InnerText))
        {
            return default;
        }
        var text = lastScriptNode.InnerText.Trim();

        OutParser.Parse(text, "document.location.href='e_university.show_notification?p1={p1}&p2={p2}&p_h={pH}&p_c_sess=1'", out string p1, out string p2, out string pH);

        if (!(string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2) || string.IsNullOrEmpty(pH)))
        {
            return new AuthorizationCredentials
            {
                Session=p2, 
                Hash=pH,
                Entry=p1
            };
        }

        return default;
    }
}