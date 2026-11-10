using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Vezza.Infra.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DataContext _context;
        public UsuarioRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAll()
        {
            return await _context.Usuarios
                    .AsNoTracking()
                    .Where(x => x.Nivel == 0)
                    .ToListAsync();
        }

        public async Task<Usuario> GetById(int usuarioId)
        {
            return await _context.Usuarios
                    .AsNoTracking()
                    .Where(x => x.UsuarioId == usuarioId && x.Nivel == 0)
                    .FirstOrDefaultAsync();
        }

        public async Task<Usuario> Save(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario> Update(Usuario usuario)
        {
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario> Delete(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }




        public async Task<Usuario> GetByEmail(string email)
        {
            return await _context.Usuarios
                    .AsNoTracking()
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();
        }
    }
}
