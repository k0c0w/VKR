using System.Transactions;
using Npgsql;

namespace DataAccess;

internal abstract class PostgresRepositoryBase(NpgsqlDataSource dataSource)
{
    protected NpgsqlDataSource DataSource { get; } = dataSource;

    protected async ValueTask<NpgsqlConnection> GetOpenedConnectionAsync(CancellationToken ct)
    {
        if (Transaction.Current is not null &&
            Transaction.Current.TransactionInformation.Status is TransactionStatus.Aborted)
        {
            throw new TransactionAbortedException("Transaction was aborted (probably by user cancellation request)");
        }

        var connection = await DataSource.OpenConnectionAsync(ct);

        return connection;
    }
}
