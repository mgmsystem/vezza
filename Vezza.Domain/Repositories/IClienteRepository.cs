using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> GetAll();
    Task<Cliente> GetById(int ClienteId);
    Task<Cliente> Save(Cliente cliente);
    Task<Cliente> Update(Cliente cliente);
    Task<Cliente> Delete(Cliente cliente);
}