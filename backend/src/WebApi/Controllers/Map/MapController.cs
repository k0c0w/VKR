using System.Net;
using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using Services.Map;
using UseCases.RetrieveBuildingByAddress;
using WebApi.Common.ProblemDetails;
using WebApi.Common.Validation;

namespace WebApi.Controllers.Map;

[Route("map")]
public class MapController : ControllerBase
{
    [HttpGet("building-boundaries")]
    public async Task<IResult> GetBuildingBoundariesAsync(
        [FromQuery] string? city,
        [FromQuery] string? street,
        [FromQuery] string? house,
        [FromServices] AddressDtoValidator validator,
        [FromServices] RetrieveBuildingByAddressUseCase useCase,
        CancellationToken ct)
    {
        var addressDto = new AddressDto
            { City = city?.Trim() ?? "", Street = street?.Trim() ?? "", House = house?.Trim() ?? "" };
        var validationResult = await validator.ValidateAsync(addressDto, ct);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary(),
                title: ProblemDetailsTitles.ArgumentValidationError);
        }

        var args = new RetrieveBuildingByAddressArgs(addressDto);
        var result = await useCase.RunAsync(args, ct);

        if (result.IsSuccess)
        {
            return Results.Json(result.Value);
        }

        var problemDetails = ToProblemDetails(result.Error);

        if (result.Error == MapProviderErrors.RateLimitError)
        {
            problemDetails.Status = (int)HttpStatusCode.TooManyRequests;
        }
        
        return Results.Problem(problemDetails);
    }
}