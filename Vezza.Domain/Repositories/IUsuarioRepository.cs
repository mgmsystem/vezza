using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> GetAll();
    Task<Usuario> GetById(int usuarioId);
    Task<Usuario> Save(Usuario usuario);
    Task<Usuario> Update(Usuario usuario);
    Task<Usuario> Delete(Usuario usuarioId);

    Task<Usuario> GetByEmail(string email);
}