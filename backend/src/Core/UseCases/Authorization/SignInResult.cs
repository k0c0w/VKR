using System.Collections.Immutable;

namespace UseCases.Authorization;

public sealed record SignInResult
{
    [System.Text.Json.Serialization.JsonPropertyName("roles")]
    [Newtonsoft.Json.JsonProperty("roles")]
    public required ImmutableArray<string> Roles { get; init; }
}