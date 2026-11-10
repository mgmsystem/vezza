namespace Vezza.Domain.Repositories;

public interface IUnitOfWork
{
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
}
