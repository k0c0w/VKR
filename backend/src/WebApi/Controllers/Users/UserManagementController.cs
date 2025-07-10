using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using UseCases.UserManagement;
using WebApi.Controllers.Users.Validation;

namespace WebApi.Controllers.Users;

[Route("users")]
public class UserManagementController : ControllerBase
{
    [HttpGet("")]
    public async Task<IResult> GetUsersAsync([FromServices] GetUserListUseCase useCase)
    {
        var result = await useCase.RunAsync(CancellationToken.None);

        if (result.IsFailure)
        {
            return Results.Problem(ToProblemDetails(result.Error));
        }

        return Results.Json(result.Value.Select(x => new
            { email = x.Email, roles = x.Roles.Select(x => x.ToString().ToLower()) }));
    }

    [HttpPost("")]
    public async Task<IResult> CreateUserAsync(
        [FromBody] AddUserUseCase.AddUserArgs user,
        [FromServices] AddUserArgsValidator validator,
        [FromServices] AddUserUseCase useCase,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(user, ct);
        if (!validationResult.IsValid)
        {
            return ToValidationProblemResult(validationResult);
        }
        
        var result = await useCase.RunAsync(user, ct);

        return result.IsFailure 
            ? Results.Problem(ToProblemDetails(result.Error)) 
            : Results.NoContent();
    }
    
    [HttpPut("{email}")]
    public async Task<IResult> UpdateUserAsync(
        [FromRoute] string email,
        [FromBody] short[] roles,
        [FromServices] UpdateUserArgsValidator validator,
        [FromServices] UpdateUserUseCase useCase,
        CancellationToken ct)
    {
        var user = new UpdateUserUseCase.UpdateUserArgs
        {
            Email = email,
            Roles = roles?.Cast<UserRole>().ToArray() ?? []
        };
        
        var validationResult = await validator.ValidateAsync(user, ct);
        if (!validationResult.IsValid)
        {
            return ToValidationProblemResult(validationResult);
        }
        
        var result = await useCase.RunAsync(user, ct);

        return result.IsFailure 
            ? Results.Problem(ToProblemDetails(result.Error)) 
            : Results.NoContent();
    }
    
    [HttpDelete("{email}")]
    public async Task<IResult> UpdateUserAsync(
        [FromRoute] string email,
        [FromServices] DeleteUserUseCase useCase,
        [FromServices] DeleteUserArgsValidator validator,
        CancellationToken ct)
    {
        var user = new DeleteUserUseCase.DeleteUserArgs()
        {
            Email = email
        };
        var validationResult = await validator.ValidateAsync(user, ct);
        if (!validationResult.IsValid)
        {
            return ToValidationProblemResult(validationResult);
        }
        
        var result = await useCase.RunAsync(user, ct);

        return result.IsFailure 
            ? Results.Problem(ToProblemDetails(result.Error)) 
            : Results.NoContent();
    }
}