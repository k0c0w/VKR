using Domain.Entities;
using Domain.Errors;
using ResultMonad;

namespace Domain.Services;

public interface IAuthorizationService
{
    Task<Result<User, ErrorMessage>> SignInAsync(string email, string password, CancellationToken ct);

    Task<User?> GetCurrentUserAsync();
}