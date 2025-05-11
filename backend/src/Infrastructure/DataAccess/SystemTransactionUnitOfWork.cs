using System.Transactions;
using Domain;

namespace DataAccess;

internal class SystemTransactionUnitOfWork : IUnitOfWork
{
    private TransactionScope? TransactionScope { get; set; }

    private bool IsDisposed { get; set; }

    private bool IsTransactionActive => TransactionScope is not null;

    public void Begin()
    {
        if (IsTransactionActive)
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
                Timeout = TimeSpan.FromSeconds(5)
            },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    public void Commit()
    {
        if (!IsTransactionActive)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SystemTransactionUnitOfWork));
        }

        TransactionScope?.Complete();
    }

    public void Abort()
    {
        if (!IsTransactionActive)
        {
            throw new InvalidOperationException("No active transaction to abort.");
        }

        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SystemTransactionUnitOfWork));
        }

        TransactionScope?.Dispose();
        TransactionScope = default;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        if (IsTransactionActive)
        {
            TransactionScope?.Dispose();
        }

        TransactionScope = default;
        IsDisposed = true;
    }
}