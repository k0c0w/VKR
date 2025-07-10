using System.Collections.Immutable;

namespace UseCases.Authorization;

public readonly record struct SignInResult
{
    [System.Text.Json.Serialization.JsonPropertyName("roles")]
    [Newtonsoft.Json.JsonProperty("roles")]
    public required ImmutableArray<string> Roles { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("p1")]
    [Newtonsoft.Json.JsonProperty("p1")]
    public required string EntryPageId { get; init; } 
    
    [System.Text.Json.Serialization.JsonPropertyName("p2")]
    [Newtonsoft.Json.JsonProperty("p2")]
    public required string Session { get; init; } 
    
    [System.Text.Json.Serialization.JsonPropertyName("p_h")]
    [Newtonsoft.Json.JsonProperty("p_h")]
    public required string VerificationHash { get; init; } 
}