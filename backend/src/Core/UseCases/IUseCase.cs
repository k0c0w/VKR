namespace UseCases;

public interface IUseCase<TResult>
{
    Task<TResult> RunAsync(CancellationToken cancellationToken);
}

public interface IUseCase<in TArgs, TResult> where TArgs : notnull
{
    Task<TResult> RunAsync(TArgs args, CancellationToken cancellationToken);
}
