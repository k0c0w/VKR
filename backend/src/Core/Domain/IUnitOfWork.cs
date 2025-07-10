namespace Domain;

public interface IUnitOfWork : IDisposable
{
    public ValueTask BeginAsync();

    public ValueTask CommitAsync();

    public ValueTask RollbackAsync();
}