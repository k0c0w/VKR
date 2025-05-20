using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UseCases.Plans;
using UseCases.Plans.Models;
using WebApi.Common.ProblemDetails;

namespace WebApi.Controllers.Plans;

[Route("plans")]
[ApiController]
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
        [FromServices] CreatePlanUseCase useCase,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(plan, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: ProblemDetailsTitles.ArgumentValidationError,
                statusCode: StatusCodes.Status400BadRequest);
        }
            
        var creationResult = await useCase.RunAsync(new CreatePlanUseCase.CreatePlanUseCaseArgs(plan), ct);
        
        return creationResult.IsSuccess 
            ? Results.Json(creationResult.Value, statusCode: (int)HttpStatusCode.Created) 
            : Results.Problem(ToProblemDetails(creationResult.Error));
    }
}