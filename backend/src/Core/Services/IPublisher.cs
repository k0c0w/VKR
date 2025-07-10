using ResultMonad;

namespace Services;

public interface IPublisher<TContract>
{
    Task<Result> PublishAsync(TContract contract, CancellationToken ct);
}