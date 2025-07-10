using System.Runtime.CompilerServices;
using System.Transactions;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using Npgsql;
using ResultMonad;

namespace DataAccess;

public abstract class PostgresRepositoryBase(NpgsqlDataSource dataSource, ILogger? logger)
{
    protected ILogger? Logger => logger;
    
    protected NpgsqlDataSource DataSource => dataSource;

    protected async ValueTask<Result<NpgsqlConnection, ErrorMessage>> GetOpenedConnectionAsync(CancellationToken ct)
    {
        if (Transaction.Current is not null &&
            Transaction.Current.TransactionInformation.Status is TransactionStatus.Aborted)
        {
            Logger?.LogInformation("Database error:Transaction was aborted (probably by user cancellation request)");
            return Result.Fail<NpgsqlConnection, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.TransactionAborted);
        }

        // todo: retries
        try
        {
            var connection = await DataSource.OpenConnectionAsync(ct);
            return Result.Ok<NpgsqlConnection, ErrorMessage>(connection);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Database error:exception while opening connection:{message}", ex.Message);
            
            return Result.Fail<NpgsqlConnection, ErrorMessage>(ErrorMessage.AbstractError);
        }
    }

    protected void LogError(Exception ex, [CallerMemberName] string? calledFromMethodName = default)
    {
        Logger?.LogError(ex, "Database error:{method}:{message}", calledFromMethodName, ex.Message);
    }
}
