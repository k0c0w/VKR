using Domain.Errors;
using Domain.Services;
using ResultMonad;

namespace UseCases.Authorization;

public class SignInUseCase(IAuthorizationService authorizationService)
    : IUseCase<SignInUseCase.SignInArgs, Result<SignInResult, ErrorMessage>>
{
    public sealed record SignInArgs(string Email, string Password);
    
    public async Task<Result<SignInResult, ErrorMessage>> RunAsync(SignInArgs args, CancellationToken ct)
    {
        var signInResult = await authorizationService.SignInAsync(args.Email, args.Password, ct);

        return signInResult.IsFailure
            ? Result.Fail<SignInResult, ErrorMessage>(signInResult.Error)
            : Result.Ok<SignInResult, ErrorMessage>(new SignInResult
            {
                Roles = [
                    ..signInResult.Value!.Roles
                        .Select(r => r.ToString())
                ]
            });
    }
}