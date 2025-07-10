using System.Net;
using DataAccess.Abstractions;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Implementation.PlanAnalyzer;
using WebApi.Common.ProblemDetails;
using WebApi.Controllers.Ai.Validation;

namespace WebApi.Controllers.Ai;

[Route("ai")]
[Authorize]
public class AiController : ControllerBase
{
    [HttpPost("indoor-plans")]
    public async Task<IResult> RegisterPlanLabelingRequestAsync(
        [FromForm] IFormFile planImage,
        [FromServices] ImageValidator validator,
        [FromServices] IPublisher<PlanImageMessage> bus, 
        [FromServices] IPlanImageAnalysisRepository storage,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(planImage, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(
                validationResult.ToDictionary(),
                title: ProblemDetailsTitles.ArgumentValidationError,
                statusCode: StatusCodes.Status400BadRequest);
        }

        var memoryStream = new MemoryStream();
        await using var imageStream = planImage.OpenReadStream();
        await imageStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        var requestId = Guid.NewGuid().ToString();
        var sendResult = await bus.PublishAsync(new PlanImageMessage(requestId, memoryStream), ct);
        if (sendResult.IsFailure)
        {
            return Results.Problem(
                detail: "Не удалось отправить изображение в обработку. Возможно, сервер перегужен.",
                title: ProblemDetailsTitles.ServerError,
                statusCode: (int)HttpStatusCode.BadRequest);
        }

        await storage.AddAsync(new PlanImageAnalysisResult()
        {
            RequestId = requestId,
            Status = PlanImageAnalysisResult.PlanImageAnalysisStatus.Pending
        }, ct);
            
        return Results.Accepted($"/ai/indoor-plans/{requestId}", new {requestId=requestId});
    }

    [HttpGet("indoor-plans/{requestId}")]
    public async Task<IResult> GetRequestIdStatusOrResultAsync(
        [FromRoute] string requestId, 
        [FromServices] IPlanImageAnalysisRepository repository,
        CancellationToken ct)
    {
        var result = await repository.GetAsync(requestId, ct);

        return result is not null 
            ? Results.Json(result)
            : Results.Problem(detail:"Операция не запущена или устарела.", 
                statusCode:(int)HttpStatusCode.NotFound, 
                title:ProblemDetailsTitles.NotFoundError);
    }
}