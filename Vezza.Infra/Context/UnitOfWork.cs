using Microsoft.EntityFrameworkCore.Storage;
using Vezza.Domain.Repositories;

namespace Vezza.Infra.Context;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    private IDbContextTransaction _transaction;

    public UnitOfWork(DataContext context)
    {
        _context = context;
    }

    public async Task BeginTransaction()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Já existe uma transação ativa neste contexto.");

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        if (_transaction == null)
            throw new InvalidOperationException("Não existe uma transação ativa para commit.");

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await DisposeTransaction();
        }
    }

    public async Task RollbackTransaction()
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await DisposeTransaction();
        }
    }

    private async Task DisposeTransaction()
    {
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
