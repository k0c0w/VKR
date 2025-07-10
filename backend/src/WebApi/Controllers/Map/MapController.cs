using System.Net;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using Services.Map;
using UseCases.Map;

namespace WebApi.Controllers.Map;

[Route("map")]
public class MapController : ControllerBase
{
    [HttpGet("building-boundaries")]
    public async Task<IResult> GetBuildingBoundariesAsync(
        [FromQuery] string? address,
        [FromServices] GetBuildingBasementInformationByAddressUseCase useCase,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(address))
        {
            return Results.Problem(ToProblemDetails(ErrorMessage.ValidationError($"Параметр '{nameof(address)}' обязателен.")));
        }
        
        var args = new GetBuildingBasementInformationByAddressUseCase.GetBuildingBasementInformationByAddressUseCaseArgs(address);
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