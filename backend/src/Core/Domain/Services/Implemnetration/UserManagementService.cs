using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Services.UserManagementService;

public sealed class UserManagementService : IUserManagementService
{
    private IUserRepository UserRepository { get; }

    public UserManagementService(IUserRepository userRepository)
    {
        UserRepository = userRepository;
    }
    
    public async Task<Result<User, ErrorMessage>> CreateNewUserAtSystemAsync(User issuer, string newUserEmail, CancellationToken ct)
    {
        if (IssuerHasInsufficientPrivileges(issuer))
        {
            return Result.Fail<User, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }
        
        var newUser = new User(newUserEmail);

        var addUserResult = await UserRepository.AddAsync(newUser, ct);
        return addUserResult.IsFailure 
            ? Result.Fail<User, ErrorMessage>(addUserResult.Error) 
            : Result.Ok<User, ErrorMessage>(newUser);
    }

    public async Task<ResultWithError<ErrorMessage>> RemoveUserFromSystemAsync(User issuer, User userToRemove, CancellationToken ct)
    {
        if (IssuerHasInsufficientPrivileges(issuer))
        {
            return ResultWithError.Fail(ErrorMessage.AuthenticationErrors.AccessDenied);
        }
        
        if (issuer.Equals(userToRemove))
        {
            return ResultWithError.Fail(ErrorMessage.DomainError("Вы не можете исключить себя из системы."));
        }

        var removalResult = await UserRepository.RemoveAsync(userToRemove, ct);
        if (removalResult.IsFailure)
        {
            return removalResult;
        }

        if (userToRemove.Roles.Contains(UserRole.Root))
        {
            var anyOtherRootInSystem = await UserRepository.AnyUserWithRole(UserRole.Root, ct);
            if (anyOtherRootInSystem.IsFailure)
            {
                return ResultWithError.Fail(ErrorMessage.DomainError($"Не удалось удалить пользователя {userToRemove.Email}."));
            }

            var rootExistance = anyOtherRootInSystem.Value;
            if (!rootExistance)
            {
                return ResultWithError.Fail<ErrorMessage>(ErrorMessage.DomainError($"Пользователь {userToRemove.Email} не может быть удалён, так как он единственный обладатель роли {nameof(UserRole.Root).ToLower()}."));
            }
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    }
    
    private static bool IssuerHasInsufficientPrivileges(User issuer)
        => !issuer.Roles.Contains(UserRole.Moderator) || !issuer.Roles.Contains(UserRole.Root);
}