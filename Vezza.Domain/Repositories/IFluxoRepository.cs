using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IFluxoRepository
{
    Task<IEnumerable<Fluxo>> GetAll();
    Task<Fluxo> GetById(int fluxoId);
    Task<Fluxo> Save(Fluxo fluxo);
    Task<Fluxo> Update(Fluxo fluxo);
    Task<Fluxo> Delete(Fluxo fluxo);
    Task<int> DeleteByOrcamentoId(int orcamentoId);
}
