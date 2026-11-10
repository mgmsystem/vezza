using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IObraRepository
{
    Task<IEnumerable<Obra>> GetAll();
    Task<Obra> GetById(int obraId);
    Task<Obra> Save(Obra obra);
    Task<Obra> Update(Obra obra);
    Task<Obra> Delete(Obra obra);
    Task<Obra> GetFirstByClienteId(int clienteId);
}
