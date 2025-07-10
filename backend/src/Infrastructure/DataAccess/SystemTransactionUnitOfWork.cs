using System.Transactions;
using Domain;

namespace DataAccess;

internal class SystemTransactionUnitOfWork : IUnitOfWork
{
    private TransactionScope? TransactionScope { get; set; }

    private bool IsDisposed { get; set; }

    public ValueTask BeginAsync()
    {
        if (TransactionScope is not null)
        {
            throw new InvalidOperationException("A transaction is already active.");
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SystemTransactionUnitOfWork));
        }

        TransactionScope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(60)
            },
            TransactionScopeAsyncFlowOption.Enabled);
        
        return ValueTask.CompletedTask;
    }

    public ValueTask CommitAsync()
    {
        if (TransactionScope is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SystemTransactionUnitOfWork));
        }

        TransactionScope.Complete();
        TransactionScope.Dispose();
        TransactionScope = default;
        
        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync()
    {
        if (TransactionScope is null)
        {
            throw new InvalidOperationException("No active transaction to rollback.");
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SystemTransactionUnitOfWork));
        }

        TransactionScope.Dispose();
        TransactionScope = default;
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        TransactionScope?.Dispose();

        TransactionScope = default;
        IsDisposed = true;
    }
}