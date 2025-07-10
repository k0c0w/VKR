using Domain.Entities;

namespace Services.Authorization;

public readonly record struct AuthorizationPayload
{
    public User AuthenticatedUser { get; init; }
    
    public string Session { get; init; }
    
    public string EntryPageId { get; init; }
    
    public string VerificationHash { get; init; }
}
