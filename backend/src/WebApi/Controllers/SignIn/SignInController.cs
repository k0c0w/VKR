using Microsoft.AspNetCore.Mvc;
using UseCases.Authorization;
using WebApi.Controllers.SignIn.Validation;

namespace WebApi.Controllers.SignIn;

[Route("authorization")]
public class SignInController : ControllerBase
{
    [HttpPost("sign-in")]
    public async Task<IResult> SignInAsync(
        [FromBody] SignInUseCase.SignInArgs payload,
        [FromServices] SignInUseCase useCase,
        [FromServices] SignInArgsValidator validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(payload, ct);
        if (!validationResult.IsValid)
        {
            return ToValidationProblemResult(validationResult);
        }

        var signInResult = await useCase.RunAsync(payload, ct);

        return signInResult.IsSuccess 
            ? Results.Json(signInResult.Value!) 
            : Results.Problem(ToProblemDetails(signInResult.Error));
    }
}