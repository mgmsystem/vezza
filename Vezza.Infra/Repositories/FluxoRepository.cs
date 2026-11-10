using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Vezza.Infra.Queries;
using Microsoft.EntityFrameworkCore.Storage;

namespace Vezza.Infra.Repositories;

public class FluxoRepository : IFluxoRepository
{
    private readonly DataContext _context;

    public FluxoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Fluxo> Delete(Fluxo fluxo)
    {
        _context.Fluxos.Remove(fluxo);
        await _context.SaveChangesAsync();

        return fluxo;
    }

    public async Task<IEnumerable<Fluxo>> GetAll()
    {
        return await _context.Fluxos
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Fluxo> GetById(int fluxoId)
    {
        return await _context.Fluxos
                .AsNoTracking()
                .Where(x => x.FluxoId == fluxoId)
                .FirstOrDefaultAsync();
    }

    public async Task<Fluxo> Save(Fluxo fluxo)
    {
        await _context.Fluxos.AddAsync(fluxo);
        await _context.SaveChangesAsync();

        return fluxo;
    }

    public async Task<Fluxo> Update(Fluxo fluxo)
    {
        _context.Entry(fluxo).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return fluxo;
    }

    public async Task<int> DeleteByOrcamentoId(int orcamentoId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var sql = FluxoQuerie.DeleteByOrcamentoId();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new { OrcamentoId = orcamentoId },
            transaction);

        return rowsAffected;
    }
}
