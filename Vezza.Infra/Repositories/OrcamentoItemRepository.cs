using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Vezza.Infra.Queries;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;

namespace Vezza.Infra.Repositories;

public class OrcamentoItemRepository : IOrcamentoItemRepository
{
    private readonly DataContext _context;

    public OrcamentoItemRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<OrcamentoItem> Delete(OrcamentoItem orcamentoItem)
    {
        _context.OrcamentoItens.Remove(orcamentoItem);
        await _context.SaveChangesAsync();

        return orcamentoItem;
    }

    public async Task<IEnumerable<OrcamentoItem>> GetAll()
    {
        return await _context.OrcamentoItens
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<OrcamentoItem> GetById(int orcamentoItemId)
    {
        return await _context.OrcamentoItens
                .AsNoTracking()
                .Where(x => x.OrcamentoItemId == orcamentoItemId)
                .FirstOrDefaultAsync();
    }

    public async Task<OrcamentoItem> Save(OrcamentoItem orcamentoItem)
    {
        await _context.OrcamentoItens.AddAsync(orcamentoItem);
        await _context.SaveChangesAsync();

        return orcamentoItem;
    }

    public async Task<OrcamentoItem> Update(OrcamentoItem orcamentoItem)
    {
        _context.Entry(orcamentoItem).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return orcamentoItem;
    }

    public async Task<int> DeleteByOrcamentoId(int orcamentoId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var sql = OrcamentoItemQuerie.DeleteByOrcamentoId();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new { OrcamentoId = orcamentoId },
            transaction);

        return rowsAffected;
    }
}
