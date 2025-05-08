using Microsoft.AspNetCore.Mvc;
using UseCases.Plans;

namespace WebApi.Endpoints.Plans;

internal static partial class HandlerRegistry
{
    internal static void UseMapEndpoints(this WebApplication app)
    {
        var plans = app.MapGroup("plans");

        plans.MapGet("", async ([FromServices] GetAllPlansMinimalDescriptionUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.RunAsync(ct);

            return Results.Json(result);
        });

        plans.MapPost("{id}", ([FromRoute] string id) =>
        {
            
        });

        plans.MapGet("{id}", async ([FromRoute] string id, 
            [FromServices] GetPlanUseCase useCase,
            CancellationToken ct) =>
        {
            var result = await useCase.RunAsync(new GetPlanUseCaseArgs(id), ct);
        });
    }
}