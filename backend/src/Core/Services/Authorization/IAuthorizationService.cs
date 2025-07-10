using Domain.Entities;
using Domain.Errors;
using ResultMonad;

namespace Services.Authorization;

public interface IAuthorizationService
{
    Task<Result<AuthorizationPayload, ErrorMessage>> SignInAsync(string email, string password, CancellationToken ct);

    Task<User?> GetCurrentUserAsync();
}