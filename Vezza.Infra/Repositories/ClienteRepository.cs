using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vezza.Infra.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly DataContext _context;
        public ClienteRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Cliente> Delete(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return cliente;
        }

        public async Task<IEnumerable<Cliente>> GetAll()
        {
            return await _context.Clientes
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<Cliente> GetById(int ClienteId)
        {
            return await _context.Clientes
                    .AsNoTracking()
                    .Where(x => x.ClienteId == ClienteId)
                    .FirstOrDefaultAsync();
        }

        public async Task<Cliente> Save(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            return cliente;
        }

        public async Task<Cliente> Update(Cliente cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return cliente;
        }
    }
}
