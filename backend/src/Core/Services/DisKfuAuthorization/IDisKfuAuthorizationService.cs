using Domain.Errors;
using ResultMonad;

namespace Services.DisKfuAuthorization;

public interface IDisKfuAuthorizationService
{
    Task<Result<AuthorizationCredentials, ErrorMessage>> SignInAsync(string login, string password, CancellationToken ct);
}