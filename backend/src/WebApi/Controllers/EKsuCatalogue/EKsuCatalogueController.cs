using Domain.Errors;
using Microsoft.AspNetCore.Mvc;
using ResultMonad;
using Services.EKsu;

namespace WebApi.Controllers.EKsuCatalogue;

[Route("catalogues")]
public class EKsuCatalogueController : ControllerBase
{
    [HttpGet("regions")]
    public async Task<IResult> GetAvailableRegionsAsync([FromServices] IBuildingCatalogue catalogue, CancellationToken ct)
    {
        var result = await catalogue.GetRegionsAsync(ct);
        return ResultOrProblem(result);
    }
    
    [HttpGet("buildings")]
    public async Task<IResult> GetAvailableBuildingsAsync(
        [FromServices] IBuildingCatalogue catalogue, 
        CancellationToken ct,
        [FromQuery] string region,
        [FromQuery] string? name = default,
        [FromQuery] string? address = default)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            var error = ErrorMessage.ValidationError($"{nameof(region)} обязательно.");
            return Results.Problem(ToProblemDetails(error));
        }
        
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address))
        {
            var result = await catalogue.GetBuildingsAsync(region, ct);
            return ResultOrProblem(result);
        }

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address))
        {
            var error = ErrorMessage.ValidationError(
                $"{nameof(name)} и {nameof(address)} должны быть представлены вместе.");
            return Results.Problem(ToProblemDetails(error));
        }

        var concreteBuildingResult = await catalogue.GetBuildingAsync(region, name, address, ct);
        return ResultOrProblem(concreteBuildingResult);
    }

    [HttpGet("it-equipment")]
    public async Task<IResult> GetItEquipmentInstalledAtRoomsAsync([FromServices] IItEquipmentCatalogue catalogue, 
        [FromQuery] long[] roomId)
    {
        if (HttpContext.User.Identity is not { IsAuthenticated: true })
        {
            return Results.Unauthorized();
        }
        
        if (roomId.Length == 0)
        {
            var error = ErrorMessage.ValidationError("Укажите хотябы одну комнату для поиска.");
            return Results.Problem(ToProblemDetails(error));
        }

        var result = await catalogue.GetAllItEquipmentByRoomIdsAsync(roomId, default);
        if (result.IsSuccess)
        {
            return Results.Json(result.Value
                .GroupBy(x => x.LocationAudienceCatalogueId)
                .ToDictionary(x => x.Key, x => x)
            );
        }

        return Results.Problem(ToProblemDetails(result.Error));
    }
    
    private IResult ResultOrProblem<T>(Result<T, ErrorMessage> result) 
        => result.IsSuccess
        ? Results.Json(result.Value)
        : Results.Problem(ToProblemDetails(result.Error)); 
}