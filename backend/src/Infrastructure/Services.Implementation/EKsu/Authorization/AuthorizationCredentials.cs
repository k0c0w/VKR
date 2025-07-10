using Domain.Entities;
using Services.Authorization;

namespace Services.Implementation.EKsu.Authorization;

internal readonly record struct AuthorizationCredentials
{
    public required string Session { get; init; }
    
    public required string Entry { get; init; }
    
    public required string Hash { get; init; }
}

internal static class AuthorizationCredentialsExtensions
{
    public static AuthorizationPayload ToAuthorizationPayloadWithUser(this AuthorizationCredentials c, User user)
    {
        return new AuthorizationPayload
        {
            AuthenticatedUser = user,
            Session = c.Session,
            VerificationHash = c.Hash,
            EntryPageId = c.Entry,
        };
    }
}