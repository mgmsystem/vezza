using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Vezza.Infra.Repositories
{
    public class AutorizacaoRepository : IAutorizacaoRepository
    {
        private readonly DataContext _context;
        public AutorizacaoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Autorizacao> Delete(Autorizacao autorizacao)
        {
            _context.Autorizacoes.Remove(autorizacao);
            await _context.SaveChangesAsync();

            return autorizacao;
        }

        public async Task<Autorizacao> GetLastByEmail(string email, string tipo)
        {
            return await _context.Autorizacoes
                    .AsNoTracking()
                    .Where(x => x.Email == email && x.Tipo == tipo)
                    .OrderByDescending(x => x.Validade)
                    .FirstOrDefaultAsync();
        }

        public async Task<Autorizacao> GetLastByEmailForUpdate(string email, string tipo)
        {
            if (_context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("A leitura bloqueante exige uma transação ativa.");

            return await _context.Autorizacoes
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM Autorizacao
                    WHERE Email = {email} AND Tipo = {tipo}
                    ORDER BY Validade DESC
                    LIMIT 1
                    FOR UPDATE")
                .FirstOrDefaultAsync();
        }

        public async Task<Autorizacao> Save(Autorizacao autorizacao)
        {
            await _context.Autorizacoes.AddAsync(autorizacao);
            await _context.SaveChangesAsync();

            return autorizacao;
        }

        public async Task<Autorizacao> Update(Autorizacao autorizacao)
        {
            _context.Entry(autorizacao).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return autorizacao;
        }
    }
}
