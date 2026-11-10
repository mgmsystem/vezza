using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Vezza.Infra.Repositories;

public class ObraRepository : IObraRepository
{
    private readonly DataContext _context;

    public ObraRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Obra> Delete(Obra obra)
    {
        _context.Obras.Remove(obra);
        await _context.SaveChangesAsync();

        return obra;
    }

    public async Task<IEnumerable<Obra>> GetAll()
    {
        return await _context.Obras
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Obra> GetById(int obraId)
    {
        return await _context.Obras
                .AsNoTracking()
                .Where(x => x.ObraId == obraId)
                .FirstOrDefaultAsync();
    }

    public async Task<Obra> Save(Obra obra)
    {
        await _context.Obras.AddAsync(obra);
        await _context.SaveChangesAsync();

        return obra;
    }

    public async Task<Obra> Update(Obra obra)
    {
        _context.Entry(obra).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return obra;
    }

    public async Task<Obra> GetFirstByClienteId(int clienteId)
    {
        return await _context.Obras
                .AsNoTracking()
                .Where(x => x.ClienteId == clienteId)
                .FirstOrDefaultAsync();
    }
}
