using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Shared.Security.Services;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Services;

namespace Vezza.Tests.Repositories;

internal class FakeClienteRepository : IClienteRepository
{
    public bool ThrowOnSave { get; set; }
    public bool DeleteCalled { get; private set; }
    public Cliente Cliente { get; set; } = new Cliente { ClienteId = 1 };

    public Task<IEnumerable<Cliente>> GetAll()
    {
        return Task.FromResult<IEnumerable<Cliente>>(new[] { Cliente });
    }

    public Task<Cliente> GetById(int clienteId)
    {
        return Task.FromResult(Cliente);
    }

    public Task<Cliente> Save(Cliente cliente)
    {
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("detalhe interno");
        }

        Cliente = cliente;
        return Task.FromResult(cliente);
    }

    public Task<Cliente> Update(Cliente cliente)
    {
        Cliente = cliente;
        return Task.FromResult(cliente);
    }

    public Task<Cliente> Delete(Cliente cliente)
    {
        DeleteCalled = true;
        return Task.FromResult(cliente);
    }
}

internal class FakeObraRepository : IObraRepository
{
    public bool ThrowOnSave { get; set; }
    public bool DeleteCalled { get; private set; }
    public Obra Obra { get; set; } = new Obra { ObraId = 1, ClienteId = 1 };
    public Obra ObraVinculada { get; set; }

    public Task<IEnumerable<Obra>> GetAll()
    {
        return Task.FromResult<IEnumerable<Obra>>(new[] { Obra });
    }

    public Task<Obra> GetById(int obraId)
    {
        return Task.FromResult(Obra);
    }

    public Task<Obra> Save(Obra obra)
    {
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("detalhe interno");
        }

        Obra = obra;
        return Task.FromResult(obra);
    }

    public Task<Obra> Update(Obra obra)
    {
        Obra = obra;
        return Task.FromResult(obra);
    }

    public Task<Obra> Delete(Obra obra)
    {
        DeleteCalled = true;
        return Task.FromResult(obra);
    }

    public Task<Obra> GetFirstByClienteId(int clienteId)
    {
        return Task.FromResult(ObraVinculada);
    }
}

internal class FakeOrcamentoRepository : IOrcamentoRepository
{
    public bool ThrowOnSave { get; set; }
    public bool UpdateCalled { get; private set; }
    public bool DeleteCalled { get; private set; }
    public bool ThrowOnDelete { get; set; }
    public Orcamento Orcamento { get; set; } = new Orcamento { OrcamentoId = 1, ObraId = 1 };
    public Orcamento OrcamentoVinculado { get; set; }

    public Task<IEnumerable<Orcamento>> GetAll()
    {
        return Task.FromResult<IEnumerable<Orcamento>>(new[] { Orcamento });
    }

    public Task<Orcamento> GetById(int orcamentoId)
    {
        return Task.FromResult(Orcamento);
    }

    public Task<Orcamento> Save(Orcamento orcamento)
    {
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("detalhe interno");
        }

        Orcamento = orcamento;
        return Task.FromResult(orcamento);
    }

    public Task<Orcamento> Update(Orcamento orcamento)
    {
        UpdateCalled = true;
        Orcamento = orcamento;
        return Task.FromResult(orcamento);
    }

    public Task<Orcamento> Delete(Orcamento orcamento)
    {
        DeleteCalled = true;

        if (ThrowOnDelete)
        {
            throw new InvalidOperationException("falha ao excluir orçamento");
        }

        return Task.FromResult(orcamento);
    }

    public Task<Orcamento> GetFirstByObraId(int obraId)
    {
        return Task.FromResult(OrcamentoVinculado);
    }
}

internal class FakeOrcamentoItemRepository : IOrcamentoItemRepository
{
    public bool DeleteByOrcamentoIdCalled { get; private set; }
    public bool ThrowOnDeleteByOrcamentoId { get; set; }

    public Task<IEnumerable<OrcamentoItem>> GetAll()
    {
        return Task.FromResult<IEnumerable<OrcamentoItem>>(Array.Empty<OrcamentoItem>());
    }

    public Task<OrcamentoItem> GetById(int orcamentoItemId)
    {
        return Task.FromResult<OrcamentoItem>(null);
    }

    public Task<OrcamentoItem> Save(OrcamentoItem orcamento)
    {
        return Task.FromResult(orcamento);
    }

    public Task<OrcamentoItem> Update(OrcamentoItem orcamento)
    {
        return Task.FromResult(orcamento);
    }

    public Task<OrcamentoItem> Delete(OrcamentoItem orcamento)
    {
        return Task.FromResult(orcamento);
    }

    public Task<int> DeleteByOrcamentoId(int orcamentoId)
    {
        DeleteByOrcamentoIdCalled = true;

        if (ThrowOnDeleteByOrcamentoId)
        {
            throw new InvalidOperationException("falha ao excluir itens");
        }

        return Task.FromResult(0);
    }
}

internal class FakeFluxoRepository : IFluxoRepository
{
    public bool DeleteByOrcamentoIdCalled { get; private set; }
    public bool ThrowOnDeleteByOrcamentoId { get; set; }

    public Task<IEnumerable<Fluxo>> GetAll()
    {
        return Task.FromResult<IEnumerable<Fluxo>>(Array.Empty<Fluxo>());
    }

    public Task<Fluxo> GetById(int fluxoId)
    {
        return Task.FromResult<Fluxo>(null);
    }

    public Task<Fluxo> Save(Fluxo fluxo)
    {
        return Task.FromResult(fluxo);
    }

    public Task<Fluxo> Update(Fluxo fluxo)
    {
        return Task.FromResult(fluxo);
    }

    public Task<Fluxo> Delete(Fluxo fluxo)
    {
        return Task.FromResult(fluxo);
    }

    public Task<int> DeleteByOrcamentoId(int orcamentoId)
    {
        DeleteByOrcamentoIdCalled = true;

        if (ThrowOnDeleteByOrcamentoId)
        {
            throw new InvalidOperationException("falha ao excluir fluxos");
        }

        return Task.FromResult(0);
    }
}

internal class ThrowingUsuarioRepository : IUsuarioRepository
{
    public Task<IEnumerable<Usuario>> GetAll()
    {
        throw new NotSupportedException();
    }

    public Task<Usuario> GetById(int usuarioId)
    {
        throw new NotSupportedException();
    }

    public Task<Usuario> Save(Usuario usuario)
    {
        throw new InvalidOperationException("detalhe interno");
    }

    public Task<Usuario> Update(Usuario usuario)
    {
        throw new NotSupportedException();
    }

    public Task<Usuario> Delete(Usuario usuarioId)
    {
        throw new NotSupportedException();
    }

    public Task<Usuario> GetByEmail(string email)
    {
        return Task.FromResult<Usuario>(null);
    }
}

internal class FakeAutorizacaoRepository : IAutorizacaoRepository
{
    public Task<Autorizacao> GetLastByEmail(string email, string tipo)
    {
        return Task.FromResult<Autorizacao>(null);
    }

    public Task<Autorizacao> GetLastByEmailForUpdate(string email, string tipo)
    {
        return Task.FromResult<Autorizacao>(null);
    }

    public Task<Autorizacao> Save(Autorizacao autorizacao)
    {
        return Task.FromResult(autorizacao);
    }

    public Task<Autorizacao> Update(Autorizacao autorizacao)
    {
        return Task.FromResult(autorizacao);
    }

    public Task<Autorizacao> Delete(Autorizacao autorizacaoId)
    {
        return Task.FromResult(autorizacaoId);
    }
}

internal class FakeUnitOfWork : IUnitOfWork
{
    private readonly SemaphoreSlim _transactionLock = new(1, 1);

    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }
    public int RollbackCount { get; private set; }

    public async Task BeginTransaction()
    {
        await _transactionLock.WaitAsync();
        BeginCount++;
    }

    public Task CommitTransaction()
    {
        CommitCount++;
        _transactionLock.Release();
        return Task.CompletedTask;
    }

    public Task RollbackTransaction()
    {
        RollbackCount++;
        _transactionLock.Release();
        return Task.CompletedTask;
    }
}

internal class FakeConfigurationService : IConfigurationService
{
    public string GetJwtKey()
    {
        return "test-key-with-at-least-32-characters";
    }

    public string GetApiKeyName()
    {
        return string.Empty;
    }

    public string GetApiKey()
    {
        return string.Empty;
    }
}

internal class FakeDemoModeService : IDemoModeService
{
    public bool IsDemoRequest { get; set; }
    public bool PublicRegistrationEnabled { get; set; }
    public string DemoEmail { get; set; } = "demo@vezza.app";
    public bool BlockCreates { get; set; }
    public DemoResource? LastResource { get; private set; }

    public bool IsDemoEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
               string.Equals(email, DemoEmail, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<GenericResult> ExecuteCreateAsync(
        DemoResource resource,
        Func<Task<GenericResult>> createAction)
    {
        LastResource = resource;
        return BlockCreates
            ? new GenericResult(409, Vezza.Api.Services.DemoModeService.LimitMessage)
            : await createAction();
    }
}
