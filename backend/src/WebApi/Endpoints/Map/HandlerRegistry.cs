using System.Net;
using Common.Dto;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using ResultMonad;
using Services.Map;
using UseCases;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Common.ProblemDetails;
using WebApi.Common.Validation;

namespace WebApi.Endpoints.Map;

internal static class HandlerRegistry
{ 
    internal static void UseMapEndpoints(this WebApplication app)
    {
        app.MapGet("/map/building-boundaries", async (
            [FromQuery] string? city,
            [FromQuery] string? street,
            [FromQuery] string? house,
            [FromServices] AddressDtoValidator validator,
            [FromServices] IUseCase<RetrieveBuildingByAddressArgs, Result<BuildingDto, ErrorMessage>> useCase, 
            CancellationToken ct) =>
        {
            var addressDto = new AddressDto(city?.Trim() ?? "", street?.Trim() ?? "", house?.Trim() ?? "");
            var validationResult = await validator.ValidateAsync(addressDto, ct);

            if (!validationResult.IsValid) 
            {
                return Results.ValidationProblem(validationResult.ToDictionary(), title: ProblemDetailsTitles.ArgumentValidationError);
            }
    
            var args = new RetrieveBuildingByAddressArgs(addressDto);
            var result = await useCase.RunAsync(args, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value);
            }
            
            var statusCode = (int)GetStatusCodeByError(result.Error);
            return Results.Problem( 
                detail: result.Error.ToString(), 
                title: ProblemDetailsTitles.DomainError, 
                statusCode:statusCode);
        });
    }

    private static HttpStatusCode GetStatusCodeByError(ErrorMessage error)
    {
        if (error == MapProviderErrors.RateLimitError)
        {
            return HttpStatusCode.TooManyRequests;
        }

        if (error == MapProviderErrors.BuildingNotFoundError)
        {
            return HttpStatusCode.NotFound;
        }

        return HttpStatusCode.BadRequest;
    }
}