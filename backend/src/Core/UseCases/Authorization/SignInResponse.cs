using System.Collections.Immutable;
using Services.DisKfuAuthorization;

namespace UseCases.Authorization;

public sealed record SignInResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("sessionInfo")]
    [Newtonsoft.Json.JsonProperty("sessionInfo")]
    public required AuthorizationCredentials SessionInfo { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("userRoles")]
    [Newtonsoft.Json.JsonProperty("userRoles")]
    public required ImmutableArray<string> Roles { get; init; } 
}