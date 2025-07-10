using Domain.Errors;
using ResultMonad;

namespace Domain.Repositories.Common;

public interface IHaveUpdate<in TEntity>
{
    Task<ResultWithError<ErrorMessage>> UpdateAsync(TEntity entity, CancellationToken ct);
}