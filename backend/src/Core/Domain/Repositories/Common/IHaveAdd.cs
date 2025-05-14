using Domain.Errors;
using ResultMonad;

namespace Domain.Repositories.Common;

public interface IHaveAdd<in TEntity>
{
    Task<ResultWithError<ErrorMessage>> AddAsync(TEntity entity, CancellationToken ct);
}