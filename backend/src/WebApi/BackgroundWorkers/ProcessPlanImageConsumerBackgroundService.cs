using DataAccess.Abstractions;
using DataAccess.Models;
using Domain.Errors;
using ResultMonad;
using Services.Implementation.PlanAnalyzer;
using Services.PlanImageAnalyzer;

namespace WebApi.BackgroundWorkers;

public class ProcessPlanImageConsumerBackgroundService(
    InMemoryPlanImageBus bus,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessPlanImageConsumerBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await bus.ConsumeAsync(stoppingToken);
            if (request.IsSuccess)
            {
                try
                {
                    await ProcessImageAsync(request.Value, stoppingToken);
                }
                catch(Exception ex)
                {
                    logger?.LogError(ex, "Exception during image processing");
                }
            }
            else
            {
                logger?.LogInformation("failed to read from bus");
            }
        }
    }

    private async Task ProcessImageAsync(PlanImageMessage message, CancellationToken ct)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPlanImageAnalysisRepository>();
        
        var planImageAnalyzer = scope.ServiceProvider.GetRequiredService<IPlanImageAnalyzerService>();
        Result<RoomOnImage[],ErrorMessage> result;
        await using (var image = message.Image)
        {
            result = await planImageAnalyzer.FindRoomsAtImageAsync(image, ct);
        }

        await SaveResultsAsync(storage, message.RequestIdentifier, result, ct);
    }

    private static Task SaveResultsAsync(IPlanImageAnalysisRepository storage,
        string requestId,
        Result<RoomOnImage[],ErrorMessage> result,
        CancellationToken ct)
    {
        return storage.AddAsync(new PlanImageAnalysisResult
        {
            RequestId = requestId,
            Status = PlanImageAnalysisResult.PlanImageAnalysisStatus.Completed,
            Error = result.IsFailure ? result.Error.ToString() : default,
            Result = result.IsSuccess ? result.Value : null
        }, ct);
    }
}