using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases.UserManagement;

public abstract class UserManagementUseCaseBase : WithAuthorizeUseCaseBase
{
    protected IUserRepository UserRepository { get; }
    public record UserManagementArgsBase
    {
            [System.Text.Json.Serialization.JsonPropertyName("email")]
            [Newtonsoft.Json.JsonProperty("email")]
            public string Email { get; init; }
    }

    protected UserManagementUseCaseBase(
        IUserRepository userRepository,
        IAuthorizationService authorizationService)
    : base(authorizationService, persistRoles: [UserRole.Moderator], rootUserCanByPass: true)
    {
        UserRepository = userRepository;
    }

    protected async Task<Result<(User Issuer, User? UserFoundByEmail), ErrorMessage>>
        AuthorizeCurrentRequestAndFindUserByEmailFromArgsAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result.Fail<(User Issuer, User? UserFoundByEmail), ErrorMessage>(ErrorMessage.ValidationError("Передан пустой Email."));
        }
        
        var authorizationResult = await AuthorizeAsync();
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<(User Issuer, User? UserFoundByEmail), ErrorMessage>(authorizationResult.Error);
        }
        
        var issuer = await GetCurrentUserAsync();

        var userWithSameEmailFindResult =
            await UserRepository.GetAsync(IUserRepository.GetUserFilter.WithEmail(email), ct);

        User? foundUser = default;
        if (userWithSameEmailFindResult.IsFailure &&
            userWithSameEmailFindResult.Error != ErrorMessage.EntityNotfoundError)
        {
            return Result.Fail<(User Issuer, User? UserFoundByEmail), ErrorMessage>(userWithSameEmailFindResult.Error);
        }
        
        if (userWithSameEmailFindResult.IsSuccess)
        {
            foundUser = userWithSameEmailFindResult.Value;
        }
        
        return Result.Ok<(User Issuer, User? UserFoundByEmail), ErrorMessage>((issuer!, foundUser));
    }
}