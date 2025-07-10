using Domain.Entities;
using Domain.Errors;
using ResultMonad;

namespace Domain.Services;

public interface IUserManagementService
{
    Task<Result<User, ErrorMessage>> CreateNewUserAtSystemAsync(User issuer, string newUserEmail, CancellationToken ct);

    Task<ResultWithError<ErrorMessage>> RemoveUserFromSystemAsync(User issuer, User userToRemove, CancellationToken ct);
}