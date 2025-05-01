using UseCases.RetrieveBuildingByAddress;

namespace WebApi.Endpoints.Map;

internal static class WebApplicationExtensions
{
    internal static void UseMapEndpoints(this WebApplication app)
    {
        app.MapGet("/map/building-boundaries", async (RetrieveBuildingByAddressDto args, RetrieveBuildingByAddressUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.RunAsync(args, ct);

            if (result.IsSuccess)
            {
                return Results.Json(result.Value);
            }

            return Results.BadRequest(new
            {
                errors=result.Error.Select(x => x)
            });
        });
    } 
}