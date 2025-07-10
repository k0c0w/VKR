using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResultMonad;

namespace Services.Implementation.PlanAnalyzer;

public class InMemoryPlanImageBus : IPublisher<PlanImageMessage>
{
    private ILogger<IPublisher<PlanImageMessage>>? Logger { get; }
    private Channel<PlanImageMessage> Bus { get; }

    public InMemoryPlanImageBus(IOptions<PlanImageBusOptions> options, ILogger<IPublisher<PlanImageMessage>>? logger = default)
    {
        Logger = logger;

        var settings = options.Value;
        if (settings.Capacity.HasValue)
        {
            Bus = Channel.CreateBounded<PlanImageMessage>(new BoundedChannelOptions(settings.Capacity.Value)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                AllowSynchronousContinuations = true,
                SingleReader = true,
            });
        }
        else
        {
            Bus = Channel.CreateUnbounded<PlanImageMessage>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                AllowSynchronousContinuations = true
            });
        }
    }
    
    public Task<Result> PublishAsync(PlanImageMessage contract, CancellationToken _)
    {
        Logger?.LogInformation("trying to send {requestId} to consumer", contract.RequestIdentifier);
        if (Bus.Writer.TryWrite(contract))
        {
            Logger?.LogInformation("send {requestId} to consumer", contract.RequestIdentifier);
            return Task.FromResult(Result.Ok());
        }

        Logger?.LogInformation("failed to send {requestId} to consumer:reader has {count} items to read", contract.RequestIdentifier, Bus.Reader.Count);
        return Task.FromResult(Result.Fail());
    }

    public async Task<Result<PlanImageMessage>> ConsumeAsync(CancellationToken ct)
    {
        var reader = Bus.Reader;
        if (await reader.WaitToReadAsync(ct))
        {
            return Result.Ok(await reader.ReadAsync(ct));
        }

        return Result.Fail<PlanImageMessage>();
    }
    
    public record PlanImageBusOptions
    {
        public int? Capacity { get; init; }
    }
}