namespace UseCases;

public interface IUseCase
{
    Task RunAsync(CancellationToken cancellationToken); 
}

public interface IUseCase<in TArgs>
{
    Task RunAsync(TArgs args, CancellationToken cancellationToken);
}

public interface IUseCase<in TArgs, TResult>
{
    Task<TResult> RunAsync(TArgs args, CancellationToken cancellationToken);
}
