using Domain.Errors;
using ResultMonad;

namespace Domain.Repositories.Common;

public interface IHaveRemove<in TEntity>
{
    Task<ResultWithError<ErrorMessage>> RemoveAsync(TEntity entity, CancellationToken ct);
}