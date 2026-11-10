using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vezza.Infra.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private readonly DataContext _context;

    public OrcamentoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Orcamento> Delete(Orcamento orcamento)
    {
        _context.Orcamentos.Remove(orcamento);
        await _context.SaveChangesAsync();

        return orcamento;
    }

    public async Task<IEnumerable<Orcamento>> GetAll()
    {
        return await _context.Orcamentos
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Orcamento> GetById(int orcamentoId)
    {
        return await _context.Orcamentos
                .AsNoTracking()
                .Where(x => x.OrcamentoId == orcamentoId)
                .FirstOrDefaultAsync();
    }

    public async Task<Orcamento> Save(Orcamento orcamento)
    {
        await _context.Orcamentos.AddAsync(orcamento);
        await _context.SaveChangesAsync();

        return orcamento;
    }

    public async Task<Orcamento> Update(Orcamento orcamento)
    {
        _context.Entry(orcamento).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return orcamento;
    }

    public async Task<Orcamento> GetFirstByObraId(int obraId)
    {
        return await _context.Orcamentos
                .AsNoTracking()
                .Where(x => x.ObraId == obraId)
                .FirstOrDefaultAsync();
    }
}
