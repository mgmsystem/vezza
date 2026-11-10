using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Tests.Repositories;

internal class FakeUsuarioSecurityRepository : IUsuarioRepository
{
    public Usuario Usuario { get; set; }
    public bool UpdateCalled { get; private set; }

    public Task<IEnumerable<Usuario>> GetAll()
    {
        var usuarios = Usuario == null ? new List<Usuario>() : new List<Usuario> { Usuario };
        return Task.FromResult<IEnumerable<Usuario>>(usuarios);
    }

    public Task<Usuario> GetById(int usuarioId)
    {
        return Task.FromResult(Usuario);
    }

    public Task<Usuario> Save(Usuario usuario)
    {
        Usuario = usuario;
        return Task.FromResult(usuario);
    }

    public Task<Usuario> Update(Usuario usuario)
    {
        UpdateCalled = true;
        Usuario = usuario;
        return Task.FromResult(usuario);
    }

    public Task<Usuario> Delete(Usuario usuarioId)
    {
        return Task.FromResult(usuarioId);
    }

    public Task<Usuario> GetByEmail(string email)
    {
        if (Usuario == null || Usuario.Email != email)
            return Task.FromResult<Usuario>(null);

        return Task.FromResult(Usuario);
    }
}

internal class FakeAutorizacaoSecurityRepository : IAutorizacaoRepository
{
    public Autorizacao Autorizacao { get; set; }
    public int UpdateCount { get; private set; }
    public bool ThrowOnUpdate { get; set; }

    public Task<Autorizacao> GetLastByEmail(string email, string tipo)
    {
        if (Autorizacao == null || Autorizacao.Email != email || Autorizacao.Tipo != tipo)
            return Task.FromResult<Autorizacao>(null);

        return Task.FromResult(Autorizacao);
    }

    public Task<Autorizacao> GetLastByEmailForUpdate(string email, string tipo)
    {
        return GetLastByEmail(email, tipo);
    }

    public Task<Autorizacao> Save(Autorizacao autorizacao)
    {
        Autorizacao = autorizacao;
        return Task.FromResult(autorizacao);
    }

    public Task<Autorizacao> Update(Autorizacao autorizacao)
    {
        UpdateCount++;

        if (ThrowOnUpdate)
        {
            throw new InvalidOperationException("falha ao consumir autorização");
        }

        Autorizacao = autorizacao;
        return Task.FromResult(autorizacao);
    }

    public Task<Autorizacao> Delete(Autorizacao autorizacaoId)
    {
        return Task.FromResult(autorizacaoId);
    }
}

internal class FakeEmailService : IEmailService
{
    public bool SendResult { get; set; } = true;
    public bool SendCalled { get; private set; }
    public string LastBody { get; private set; }

    public Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        SendCalled = true;
        LastBody = body;
        return Task.FromResult(SendResult);
    }

    public string TemplateCodigoAutorizacao(string codigo)
    {
        return $"<b>{codigo}</b>";
    }
}
