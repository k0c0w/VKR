using Common.Dto;
using Domain.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ResultMonad;
using UseCases;
using UseCases.Plans;
using UseCases.Plans.Models;
using WebApi.Common.ProblemDetails;

namespace WebApi.Endpoints.Plans;

internal static class HandlerRegistry
{
    internal static void UsePlansEndpoints(this WebApplication app)
    {
        var plansRoute = app.MapGroup("plans");

        plansRoute.MapGet("", async ([FromServices] GetAvailablePlansListUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.RunAsync(ct);

            return result.IsSuccess ? Results.Json(result.Value, statusCode: StatusCodes.Status200OK) 
                : MapErrorToProblemDetails(result.Error);
        });

        plansRoute.MapGet("plan", async (
            [FromQuery] string city,
            [FromQuery] string street,
            [FromQuery] string house,
            [FromServices] IValidator<AddressDto> validator,
            [FromServices] IUseCase<GetPlanUseCaseArgs, Result<BuildingPlan, ErrorMessage>> useCase,
            CancellationToken ct) =>
        {
            var addressDto = new AddressDto(city?.Trim() ?? "", street?.Trim() ?? "", house?.Trim() ?? "");
            var validationResult = await validator.ValidateAsync(addressDto, ct);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(
                    validationResult.ToDictionary(),
                    title: ProblemDetailsTitles.ArgumentValidationError,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var args = new GetPlanUseCaseArgs(addressDto);
            var result = await useCase.RunAsync(args, ct);

            return result.IsSuccess ? Results.Json(result.Value, statusCode: StatusCodes.Status200OK) 
                : MapErrorToProblemDetails(result.Error);
        });
    }

    private static IResult MapErrorToProblemDetails(ErrorMessage error)
    {
        if (error == ErrorMessage.EntityNotfoundError)
        {
            return Results.Problem(
                title: ProblemDetailsTitles.DomainError,
                detail: error.ToString(),
                statusCode: StatusCodes.Status404NotFound);
        }

        if (error == ErrorMessage.AbstractError ||
            error == ErrorMessage.RepositorySpecificErrors.AddError ||
            error == ErrorMessage.RepositorySpecificErrors.GetError ||
            error == ErrorMessage.RepositorySpecificErrors.TransactionAborted)
        {
            return Results.Problem(
                title: "Internal server error.",
                detail: error.ToString(),
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Problem(
            title: ProblemDetailsTitles.DomainError,
            detail: error.ToString(),
            statusCode: StatusCodes.Status400BadRequest);
    }
}