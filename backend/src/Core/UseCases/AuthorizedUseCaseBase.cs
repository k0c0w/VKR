using System.Collections.Immutable;
using Domain.Entities;
using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases;

public abstract class WithAuthorizeUseCaseBase(
    IAuthorizationService authorizationService,
    ImmutableArray<UserRole>? persistRoles = default,
    bool rootUserCanByPass = default
    )
{
    private User? CurrentUser { get; set; }
    
    protected async ValueTask<User?> GetCurrentUserAsync()
    {
        return CurrentUser ??= await authorizationService.GetCurrentUserAsync();
    }
    
    protected async Task<ResultWithError<ErrorMessage>> AuthorizeAsync()
    {
        var currentUser = await authorizationService.GetCurrentUserAsync();
        if (currentUser is null)
        {
            return ResultWithError.Fail(ErrorMessage.AuthenticationErrors.Unauthorized);
        }

        CurrentUser = currentUser;

        if (rootUserCanByPass && currentUser.Roles.Contains(UserRole.Root))
        {
            return ResultWithError.Ok<ErrorMessage>();
        }
        
        if (persistRoles != default && persistRoles.Any<UserRole>(role => !currentUser.Roles.Contains(role)))
        {
            return ResultWithError.Fail(ErrorMessage.AuthenticationErrors.AccessDenied);
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    } 
}