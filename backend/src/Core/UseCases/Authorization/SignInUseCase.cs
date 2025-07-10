using Domain.Errors;
using ResultMonad;
using Services.Authorization;

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
                    ..signInResult.Value.AuthenticatedUser.Roles
                        .Select(r => r.ToString())
                        .Select(r => r.ToLower())
                ],
                Session = signInResult.Value.Session,
                VerificationHash = signInResult.Value.VerificationHash,
                EntryPageId = signInResult.Value.EntryPageId,
            });
    }
}