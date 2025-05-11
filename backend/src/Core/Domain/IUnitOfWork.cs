using System.Transactions;

namespace Domain;

public interface IUnitOfWork : IDisposable
{
    public void Begin();

    public void Commit();

    public void Abort();
}