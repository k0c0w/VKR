using System.Collections.Immutable;
using Domain.Errors;
using Domain.Services;
using Domain.ValueObjects;
using ResultMonad;

namespace UseCases;

public abstract class WithAuthorizeUseCaseBase(
    IAuthorizationService authorizationService,
    ImmutableArray<UserRole>? persistRoles = default
    )
{
    protected async Task<ResultWithError<ErrorMessage>> AuthorizeAsync()
    {
        var currentUser = await authorizationService.GetCurrentUserAsync();
        if (currentUser is null)
        {
            return ResultWithError.Fail(ErrorMessage.AuthenticationErrors.Unauthorized);
        }

        if (persistRoles != default && persistRoles.Any<UserRole>(role => !currentUser.Roles.Contains(role)))
        {
            return ResultWithError.Fail(ErrorMessage.AuthenticationErrors.AccessDenied);
        }
        
        return ResultWithError.Ok<ErrorMessage>();
    } 
}