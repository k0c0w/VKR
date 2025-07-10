using System.Collections.Immutable;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases.UserManagement;

public sealed class GetUserListUseCase
    : WithAuthorizeUseCaseBase, 
      IUseCase<Result<User[], ErrorMessage>>
{
    private IUserRepository UserRepository { get; }
    
    public GetUserListUseCase(IUserRepository userRepository, IAuthorizationService authorizationService)
        : base(authorizationService, persistRoles:[UserRole.Moderator],rootUserCanByPass: true)
    {
        UserRepository = userRepository;
    }
    
    public async Task<Result<User[], ErrorMessage>> RunAsync(CancellationToken ct)
    {
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<User[], ErrorMessage>(authorizationResult.Error);
        }

        var users = await UserRepository.GetAllAsync(ct);
        return users;
    }
}