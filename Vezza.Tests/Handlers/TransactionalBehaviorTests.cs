using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Entities;
using Vezza.Shared.Security.Password;
using Vezza.Shared.Security.Token;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Handlers;

[TestClass]
public class TransactionalBehaviorTests
{
    [TestMethod]
    public async Task DeleteOrcamento_ComTodasAsOperacoesConcluidas_DeveFazerCommit()
    {
        var dependencies = CreateOrcamentoDependencies();

        var result = await dependencies.Handler.Handle(new DeleteOrcamentoCommand(1));

        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dependencies.ItemRepository.DeleteByOrcamentoIdCalled);
        Assert.IsTrue(dependencies.FluxoRepository.DeleteByOrcamentoIdCalled);
        Assert.IsTrue(dependencies.OrcamentoRepository.DeleteCalled);
        Assert.AreEqual(1, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(0, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task DeleteOrcamento_ComFalhaNosItens_DeveSolicitarRollback()
    {
        var dependencies = CreateOrcamentoDependencies();
        dependencies.ItemRepository.ThrowOnDeleteByOrcamentoId = true;

        var result = await dependencies.Handler.Handle(new DeleteOrcamentoCommand(1));

        Assert.AreEqual(500, result.StatusCode);
        Assert.IsFalse(dependencies.FluxoRepository.DeleteByOrcamentoIdCalled);
        Assert.IsFalse(dependencies.OrcamentoRepository.DeleteCalled);
        Assert.AreEqual(0, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task DeleteOrcamento_ComFalhaNosFluxos_DeveSolicitarRollback()
    {
        var dependencies = CreateOrcamentoDependencies();
        dependencies.FluxoRepository.ThrowOnDeleteByOrcamentoId = true;

        var result = await dependencies.Handler.Handle(new DeleteOrcamentoCommand(1));

        Assert.AreEqual(500, result.StatusCode);
        Assert.IsTrue(dependencies.ItemRepository.DeleteByOrcamentoIdCalled);
        Assert.IsFalse(dependencies.OrcamentoRepository.DeleteCalled);
        Assert.AreEqual(0, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task DeleteOrcamento_ComFalhaNoOrcamento_DeveSolicitarRollback()
    {
        var dependencies = CreateOrcamentoDependencies();
        dependencies.OrcamentoRepository.ThrowOnDelete = true;

        var result = await dependencies.Handler.Handle(new DeleteOrcamentoCommand(1));

        Assert.AreEqual(500, result.StatusCode);
        Assert.IsTrue(dependencies.ItemRepository.DeleteByOrcamentoIdCalled);
        Assert.IsTrue(dependencies.FluxoRepository.DeleteByOrcamentoIdCalled);
        Assert.AreEqual(0, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task AlterarSenha_ComFalhaAoConsumirCodigo_DeveSolicitarRollback()
    {
        const string codigo = "123456";
        var dependencies = CreateUsuarioDependencies(codigo);
        dependencies.AutorizacaoRepository.ThrowOnUpdate = true;

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand("usuario@teste.com", "senha-nova", codigo));

        Assert.AreEqual(500, result.StatusCode);
        Assert.AreEqual(0, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task AlterarSenha_AposPrimeiroConsumo_DeveRejeitarSegundoConsumo()
    {
        const string codigo = "123456";
        var dependencies = CreateUsuarioDependencies(codigo);

        var primeiroResultado = await dependencies.Handler.Handle(
            new AlterarSenhaCommand("usuario@teste.com", "senha-nova", codigo));
        var segundoResultado = await dependencies.Handler.Handle(
            new AlterarSenhaCommand("usuario@teste.com", "outra-senha", codigo));

        Assert.AreEqual(200, primeiroResultado.StatusCode);
        Assert.AreEqual(401, segundoResultado.StatusCode);
        Assert.AreEqual(1, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    [TestMethod]
    public async Task AlterarSenha_ComDuasTentativasConcorrentes_DevePermitirApenasUmConsumo()
    {
        const string codigo = "123456";
        var dependencies = CreateUsuarioDependencies(codigo);

        var resultados = await Task.WhenAll(
            Task.Run(() => dependencies.Handler.Handle(
                new AlterarSenhaCommand("usuario@teste.com", "senha-a", codigo))),
            Task.Run(() => dependencies.Handler.Handle(
                new AlterarSenhaCommand("usuario@teste.com", "senha-b", codigo))));

        Assert.AreEqual(1, resultados.Count(x => x.StatusCode == 200));
        Assert.AreEqual(1, resultados.Count(x => x.StatusCode == 401));
        Assert.AreEqual(1, dependencies.UnitOfWork.CommitCount);
        Assert.AreEqual(1, dependencies.UnitOfWork.RollbackCount);
    }

    private static OrcamentoDependencies CreateOrcamentoDependencies()
    {
        var orcamentoRepository = new FakeOrcamentoRepository
        {
            Orcamento = new Orcamento { OrcamentoId = 1, ObraId = 1 }
        };
        var itemRepository = new FakeOrcamentoItemRepository();
        var fluxoRepository = new FakeFluxoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new OrcamentoHandler(
            orcamentoRepository,
            itemRepository,
            fluxoRepository,
            unitOfWork,
            NullLogger<OrcamentoHandler>.Instance);

        return new OrcamentoDependencies(
            handler,
            orcamentoRepository,
            itemRepository,
            fluxoRepository,
            unitOfWork);
    }

    private static UsuarioDependencies CreateUsuarioDependencies(string codigo)
    {
        var configuration = new FakeConfigurationService();
        var tokenUtils = new TokenUtils(configuration);
        var usuarioRepository = new FakeUsuarioSecurityRepository
        {
            Usuario = new Usuario
            {
                UsuarioId = 1,
                Nome = "Usuário Teste",
                Email = "usuario@teste.com",
                Senha = PasswordUtils.PasswordEncripty("senha-antiga")
            }
        };
        var autorizacaoRepository = new FakeAutorizacaoSecurityRepository
        {
            Autorizacao = new Autorizacao
            {
                AutorizacaoId = 1,
                Email = "usuario@teste.com",
                Tipo = "TrocarSenha",
                Codigo = tokenUtils.HashCodigoAutorizacao(codigo),
                Validade = DateTime.UtcNow.AddMinutes(5)
            }
        };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UsuarioHandler(
            usuarioRepository,
            autorizacaoRepository,
            tokenUtils,
            new FakeEmailService(),
            unitOfWork,
            NullLogger<UsuarioHandler>.Instance);

        return new UsuarioDependencies(handler, autorizacaoRepository, unitOfWork);
    }

    private sealed class OrcamentoDependencies
    {
        public OrcamentoDependencies(
            OrcamentoHandler handler,
            FakeOrcamentoRepository orcamentoRepository,
            FakeOrcamentoItemRepository itemRepository,
            FakeFluxoRepository fluxoRepository,
            FakeUnitOfWork unitOfWork)
        {
            Handler = handler;
            OrcamentoRepository = orcamentoRepository;
            ItemRepository = itemRepository;
            FluxoRepository = fluxoRepository;
            UnitOfWork = unitOfWork;
        }

        public OrcamentoHandler Handler { get; }
        public FakeOrcamentoRepository OrcamentoRepository { get; }
        public FakeOrcamentoItemRepository ItemRepository { get; }
        public FakeFluxoRepository FluxoRepository { get; }
        public FakeUnitOfWork UnitOfWork { get; }
    }

    private sealed class UsuarioDependencies
    {
        public UsuarioDependencies(
            UsuarioHandler handler,
            FakeAutorizacaoSecurityRepository autorizacaoRepository,
            FakeUnitOfWork unitOfWork)
        {
            Handler = handler;
            AutorizacaoRepository = autorizacaoRepository;
            UnitOfWork = unitOfWork;
        }

        public UsuarioHandler Handler { get; }
        public FakeAutorizacaoSecurityRepository AutorizacaoRepository { get; }
        public FakeUnitOfWork UnitOfWork { get; }
    }
}
