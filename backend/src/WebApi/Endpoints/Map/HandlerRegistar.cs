using System.Net;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using ResultMonad;
using Services.Map;
using UseCases;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Endpoints.Map;

namespace WebApi.Endpoints;

internal static partial class HandlerRegistar
{
    internal static void UseMapEndpoints(this WebApplication app)
    {
        app.MapGet("/map/building-boundaries", async (
            [FromQuery] string? city,
            [FromQuery] string? street,
            [FromQuery] string? house,
            [FromServices] RetrieveBuildingByAddressDtoValidator validator,
            [FromServices] IUseCase<RetrieveBuildingByAddressDto, Result<BuildingDto, ErrorMessage>> useCase, 
            CancellationToken ct) =>
        {
            var args = new RetrieveBuildingByAddressDto(city?.Trim() ?? "", street?.Trim() ?? "", house?.Trim() ?? "");
            var validationResult = await validator.ValidateAsync(args, ct);
            if (!validationResult.IsValid) 
            {
                return Results.ValidationProblem(validationResult.ToDictionary(), title: "Ошибка валидации.");
            }
    
            var result = await useCase.RunAsync(args, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value);
            }

            if (result.Error == MapProviderErrors.BuildingNotFoundError)
            {
                return Results.Problem(detail: result.Error.ToString(), title: "Доменная ошибка.",
                    statusCode: (int)HttpStatusCode.NotFound);
            }
            
            // todo: handle status code due to error
            return Results.Problem( 
                detail: result.Error.ToString(), title: "Доменная ошибка.", statusCode:(int)HttpStatusCode.BadRequest);
        });
    } 
}