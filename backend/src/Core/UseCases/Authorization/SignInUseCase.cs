using Domain.Errors;
using Domain.Repositories;
using ResultMonad;
using Services.DisKfuAuthorization;

namespace UseCases.Authorization;

public class SignInUseCase(
    IUserRepository userRepository,
    IDisKfuAuthorizationService authorizationService
    )
    : IUseCase<SignInUseCase.SignInArgs, Result<SignInResponse, ErrorMessage>>
{
    public sealed record SignInArgs(string Email, string Password);
    
    public async Task<Result<SignInResponse, ErrorMessage>> RunAsync(SignInArgs args, CancellationToken ct)
    {
        var userFindResult = await userRepository.GetAsync(IUserRepository.GetUserFilter.WithEmail(args.Email), ct);
        if (userFindResult.IsFailure)
        {
            return Result.Fail<SignInResponse, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var authorizationResult = await authorizationService.SignInAsync(args.Email, args.Password, ct);
        if (authorizationResult.IsFailure)
        {
            return Result.Fail<SignInResponse, ErrorMessage>(ErrorMessage.AuthenticationErrors.AccessDenied);
        }

        var response = new SignInResponse
        {
            SessionInfo = authorizationResult.Value!,
            Roles =
            [
                ..userFindResult.Value!.Roles
                    .Select(x => x.ToString())
            ]
        };
        
        return Result.Ok<SignInResponse, ErrorMessage>(response);
    }
}