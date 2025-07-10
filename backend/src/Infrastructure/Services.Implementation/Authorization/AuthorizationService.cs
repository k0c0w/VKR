using System.Security.Claims;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using ResultMonad;
using Services.Implementation.EKsu.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Services.Authorization;

namespace Services.Implementation.Authorization;

internal class AuthorizationService(
    IUserRepository userRepository,
    EKsuAuthorizationClient authorizationClient,
    IHttpContextAccessor httpContextAccessor
    )
    : IAuthorizationService
{
    public const string AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    
    public async Task<Result<AuthorizationPayload, ErrorMessage>> SignInAsync(string email, string password, CancellationToken ct)
    {
        var userFindResult = await userRepository.GetAsync(IUserRepository.GetUserFilter.WithEmail(email), ct);
        if (userFindResult.IsFailure)
        {
            return Result.Fail<AuthorizationPayload, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var signInResult = await authorizationClient.SignInAsync(email, password, ct);
        if (signInResult.IsFailure)
        {
            return Result.Fail<AuthorizationPayload, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var user = userFindResult.Value;
        await AuthenticateHttpContextAsync(user, signInResult.Value);
        var credentials = signInResult.Value;
        
        return Result.Ok<AuthorizationPayload,ErrorMessage>(credentials.ToAuthorizationPayloadWithUser(user));
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
                new Claim(nameof(AuthorizationPayload.EntryPageId), credentials.Entry),
                new Claim(nameof(AuthorizationPayload.Session), credentials.Session),
                new Claim(nameof(AuthorizationPayload.VerificationHash), credentials.Hash),
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