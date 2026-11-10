using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IOrcamentoItemRepository
{
    Task<IEnumerable<OrcamentoItem>> GetAll();
    Task<OrcamentoItem> GetById(int orcamentoItemId);
    Task<OrcamentoItem> Save(OrcamentoItem orcamento);
    Task<OrcamentoItem> Update(OrcamentoItem orcamento);
    Task<OrcamentoItem> Delete(OrcamentoItem orcamento);
    Task<int> DeleteByOrcamentoId(int orcamentoId);
}
