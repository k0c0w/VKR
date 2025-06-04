namespace Services.EKsu.Authorization;

public readonly record struct AuthorizationCredentials
{
    [System.Text.Json.Serialization.JsonPropertyName("session")]
    [Newtonsoft.Json.JsonProperty("session")]
    public required string Session { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("entry")]
    [Newtonsoft.Json.JsonProperty("entry")]
    public required string Entry { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("hash")]
    [Newtonsoft.Json.JsonProperty("hash")]
    public required string Hash { get; init; }

    public void Deconstruct(out string entry, out string session, out string hash)
    {
        entry = Entry;
        session = Session;
        hash = Hash;
    }
}