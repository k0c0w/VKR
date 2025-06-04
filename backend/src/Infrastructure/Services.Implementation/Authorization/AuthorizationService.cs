using System.Security.Claims;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Http;
using ResultMonad;
using Services.Implementation.EKsu.Authorization;
using Microsoft.AspNetCore.Authentication;
using Services.EKsu.Authorization;

namespace Services.Implementation.Authorization;

public class AuthorizationService(
    IUserRepository userRepository,
    EKsuAuthorizationClient authorizationClient,
    IHttpContextAccessor httpContextAccessor
    )
    : IAuthorizationService
{
    public const string AuthenticationScheme = "Authority";
    
    public async Task<Result<User, ErrorMessage>> SignInAsync(string email, string password, CancellationToken ct)
    {
        var userFindResult = await userRepository.GetAsync(IUserRepository.GetUserFilter.WithEmail(email), ct);
        if (userFindResult.IsFailure)
        {
            return Result.Fail<User, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var signInResult = await authorizationClient.SignInAsync(email, password, ct);
        if (signInResult.IsFailure)
        {
            return Result.Fail<User, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var user = userFindResult.Value!;
        await AuthenticateHttpContextAsync(user, signInResult.Value!);
        
        return Result.Ok<User,ErrorMessage>(user);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userPrincipal = httpContextAccessor.HttpContext?.User;
        if (userPrincipal == null || userPrincipal.Identity is { IsAuthenticated: false })
        {
            return default;
        }

        var emailClaim = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        if (emailClaim is null)
        {
            return default;
        }

        var userFindResult = await userRepository.GetAsync(IUserRepository.GetUserFilter.WithEmail(emailClaim), CancellationToken.None);
        return userFindResult.IsFailure ? default : userFindResult.Value!;
    }

    private Task AuthenticateHttpContextAsync(User user, AuthorizationCredentials credentials)
    {
        var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(nameof(AuthorizationCredentials.Entry), credentials.Entry),
                new Claim(nameof(AuthorizationCredentials.Session), credentials.Session),
                new Claim(nameof(AuthorizationCredentials.Hash), credentials.Hash),
            }
            .Concat(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        return httpContextAccessor.HttpContext.SignInAsync(
            AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                AllowRefresh = false,
            });
    }
}