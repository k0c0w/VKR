using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UseCases.Plans;
using UseCases.Plans.Models;
using UseCases.Plans.Update;
using UseCases.Plans.Update.Models;
using WebApi.Common.ProblemDetails;
using WebApi.Controllers.Plans.Validation;

namespace WebApi.Controllers.Plans;

[Route("plans")]
public sealed class PlansController : ControllerBase
{
    [HttpGet("")]
    public async Task<IResult> GetAllPlansShortDescriptionsAsync(
        [FromServices] GetAvailablePlansListUseCase useCase, 
        CancellationToken ct)
    {
        var result = await useCase.RunAsync(ct);

        return result.IsSuccess ? Results.Json(result.Value, statusCode: StatusCodes.Status200OK) 
            : Results.Problem(ToProblemDetails(result.Error));
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetPlanById(
        [FromRoute] string id,
        [FromServices] GetPlanUseCase useCase,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return Results.Problem( 
                detail: "Неизвестный uuid.", 
                title: ProblemDetailsTitles.NotFoundError, 
                statusCode: 404);
        }
            
        var result = await useCase.RunAsync(new GetPlanUseCase.GetPlanUseCaseArgs(guid), ct);

        return result.IsSuccess ? Results.Json(result.Value, statusCode: StatusCodes.Status200OK) 
            : Results.Problem(ToProblemDetails(result.Error));
    }

    [HttpPost("")]
    public async Task<IResult> CreatePlanAsync(
        [FromBody] BuildingPlan plan,
        [FromServices] IValidator<BuildingPlan> validator,
        [FromServices] CreatePlanUseCase useCase)
    {
        var validationResult = await validator.ValidateAsync(plan, default);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: ProblemDetailsTitles.ArgumentValidationError,
                statusCode: StatusCodes.Status400BadRequest);
        }
            
        var creationResult = await useCase.RunAsync(new CreatePlanUseCase.CreatePlanUseCaseArgs(plan), default);
        
        return creationResult.IsSuccess 
            ? Results.Json(creationResult.Value, statusCode: (int)HttpStatusCode.Created) 
            : Results.Problem(ToProblemDetails(creationResult.Error));
    }
    
    [HttpPatch("{id}")]
    public async Task<IResult> UpdateExistingPlanAsync(
        [FromRoute] string id,
        [FromBody] UpdateInstruction[] updates,
        [FromServices] UpdatesValidation validator,
        [FromServices] UpdateExistingPlanUseCase useCase)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return Results.Problem( 
                detail: "Неизвестный uuid.", 
                title: ProblemDetailsTitles.NotFoundError, 
                statusCode: 404);
        }
        
        var args = new UpdateExistingPlanUseCase.UpdateBuildingUseCaseArgs(guid, updates);
        var validationResult = await validator.ValidateAsync(args, default);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: ProblemDetailsTitles.ArgumentValidationError,
                statusCode: StatusCodes.Status400BadRequest);
        }

        var updateResult = await useCase.RunAsync(args, default);
        
        return updateResult.IsSuccess 
            ? Results.Json(updateResult.Value) 
            : Results.Problem(ToProblemDetails(updateResult.Error));
    }

    [HttpDelete("{planId:guid}")]
    public async Task<IResult> DeletePlanAsync([FromRoute] Guid planId, [FromServices] DeletePlanArgsValidator validator, [FromServices] DeletePlanUseCase useCase)
    {
        var args = new DeletePlanUseCase.DeletePlanUseCaseArgs(planId);
        var validationResult = await validator.ValidateAsync(args);
        if (!validationResult.IsValid)
        {
            return ToValidationProblemResult(validationResult);
        }

        var deletionResult = await useCase.RunAsync(args, CancellationToken.None);
        if (deletionResult.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(ToProblemDetails(deletionResult.Error));
    }
}