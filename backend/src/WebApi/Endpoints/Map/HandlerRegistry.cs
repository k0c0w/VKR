using System.Net;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using ResultMonad;
using UseCases;
using UseCases.RetrieveBuildingByAddress;

namespace WebApi.Endpoints.Map;

internal static class HandlerRegistry
{
    internal static void UseMapEndpoints(this WebApplication app)
    {
        app.MapGet("/map/building-boundaries", async (
            [FromQuery] string city,
            [FromQuery] string street,
            [FromQuery] string house,
            [FromServices] RetrieveBuildingByAddressDtoValidator validator,
            [FromServices] IUseCase<RetrieveBuildingByAddressDto, Result<BuildingDto, ErrorMessage>> useCase, 
            CancellationToken ct) =>
        {
            var args = new RetrieveBuildingByAddressDto(city.Trim(), street.Trim(), house.Trim());
            var validationResult = await validator.ValidateAsync(args, ct);
            if (!validationResult.IsValid) 
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
    
            var result = await useCase.RunAsync(args, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value);
            }

            // todo: handle status code due to error
            return Results.Problem(detail: result.Error, title: "Domain error", statusCode:(int)HttpStatusCode.BadRequest);
        });
    } 
}