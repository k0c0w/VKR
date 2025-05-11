namespace Domain.Repositories.Common;

public interface IHaveAdd<in TEntity>
{
    Task AddAsync(TEntity entity, CancellationToken ct);
}