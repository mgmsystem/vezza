using Vezza.Domain.Entities;

namespace Vezza.Domain.Repositories;
public interface IAutorizacaoRepository
{
    Task<Autorizacao> GetLastByEmail(string email, string tipo);
    Task<Autorizacao> GetLastByEmailForUpdate(string email, string tipo);
    Task<Autorizacao> Save(Autorizacao autorizacao);
    Task<Autorizacao> Update(Autorizacao autorizacao);
    Task<Autorizacao> Delete(Autorizacao autorizacaoId);
}
