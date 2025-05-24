using Domain.Errors;
using ResultMonad;

namespace Services.PlanImageAnalyzer;

public interface IPlanImageAnalyzerService
{
    Task<Result<RoomOnImage[], ErrorMessage>> FindRoomsAtImageAsync(Stream image, CancellationToken ct);
}