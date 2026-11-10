using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IOrcamentoRepository
{
    Task<IEnumerable<Orcamento>> GetAll();
    Task<Orcamento> GetById(int orcamentoId);
    Task<Orcamento> Save(Orcamento orcamento);
    Task<Orcamento> Update(Orcamento orcamento);
    Task<Orcamento> Delete(Orcamento orcamento);
    Task<Orcamento> GetFirstByObraId(int obraId);    
}
