using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Domain.CQRS.Commands.Clientes;
using Vezza.Domain.CQRS.Commands.Obras;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Entities;
using Vezza.Domain.Services;
using Vezza.Shared.Security.Token;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Handlers;

[TestClass]
public class HandlerBehaviorTests
{
    [TestMethod]
    public async Task DeleteCliente_ComObraVinculada_DeveRetornarBadRequestSemExcluir()
    {
        var clienteRepository = new FakeClienteRepository();
        var obraRepository = new FakeObraRepository
        {
            ObraVinculada = new Obra { ObraId = 10, ClienteId = 1 }
        };
        var handler = new ClienteHandler(
            clienteRepository,
            obraRepository,
            NullLogger<ClienteHandler>.Instance);

        var result = await handler.Handle(new DeleteClienteCommand(1));

        Assert.AreEqual(400, result.StatusCode);
        Assert.IsFalse(clienteRepository.DeleteCalled);
        CollectionAssert.Contains(result.Errors, "Cliente vinculado a obras não pode ser excluído");
    }

    [TestMethod]
    public async Task DeleteObra_ComOrcamentoVinculado_DeveRetornarBadRequestSemExcluir()
    {
        var obraRepository = new FakeObraRepository();
        var orcamentoRepository = new FakeOrcamentoRepository
        {
            OrcamentoVinculado = new Orcamento { OrcamentoId = 10, ObraId = 1 }
        };
        var handler = new ObraHandler(
            obraRepository,
            orcamentoRepository,
            NullLogger<ObraHandler>.Instance);

        var result = await handler.Handle(new DeleteObraCommand(1));

        Assert.AreEqual(400, result.StatusCode);
        Assert.IsFalse(obraRepository.DeleteCalled);
        CollectionAssert.Contains(result.Errors, "Obra vinculada a orçamentos não pode ser excluída");
    }

    [TestMethod]
    public async Task UpdateOrcamento_ComNovaData_DevePersistirDataRecebida()
    {
        var dataAnterior = new DateTime(2024, 1, 10);
        var novaData = new DateTime(2025, 6, 20);
        var repository = new FakeOrcamentoRepository
        {
            Orcamento = new Orcamento { OrcamentoId = 1, ObraId = 1, Data = dataAnterior }
        };
        var handler = new OrcamentoHandler(
            repository,
            new FakeOrcamentoItemRepository(),
            new FakeFluxoRepository(),
            new FakeUnitOfWork(),
            NullLogger<OrcamentoHandler>.Instance);
        var command = new UpdateOrcamentoCommand(
            1, 1, novaData, "Proposta atualizada", 200m, "À vista", "30 dias", "", "",
            novaData, novaData.AddDays(30), "Aberto");

        var result = await handler.Handle(command);

        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(repository.UpdateCalled);
        Assert.AreEqual(novaData, repository.Orcamento.Data);
    }

    [TestMethod]
    public async Task ClienteHandler_ExcecaoSemInnerException_DeveRetornarErroInterno()
    {
        var repository = new FakeClienteRepository { ThrowOnSave = true };
        var handler = new ClienteHandler(
            repository,
            new FakeObraRepository(),
            NullLogger<ClienteHandler>.Instance);
        var command = new CreateClienteCommand(
            "Cliente Teste", "12345678900", "", "", "", "", "", "", "", "");

        var result = await handler.Handle(command);

        AssertInternalServerError(result);
    }

    [TestMethod]
    public async Task ObraHandler_ExcecaoSemInnerException_DeveRetornarErroInterno()
    {
        var repository = new FakeObraRepository { ThrowOnSave = true };
        var handler = new ObraHandler(
            repository,
            new FakeOrcamentoRepository(),
            NullLogger<ObraHandler>.Instance);
        var command = new CreateObraCommand(
            1, "Obra Teste", "", "", "", "", "", "", "");

        var result = await handler.Handle(command);

        AssertInternalServerError(result);
    }

    [TestMethod]
    public async Task OrcamentoHandler_ExcecaoSemInnerException_DeveRetornarErroInterno()
    {
        var repository = new FakeOrcamentoRepository { ThrowOnSave = true };
        var handler = new OrcamentoHandler(
            repository,
            new FakeOrcamentoItemRepository(),
            new FakeFluxoRepository(),
            new FakeUnitOfWork(),
            NullLogger<OrcamentoHandler>.Instance);
        var command = new CreateOrcamentoCommand(
            1, DateTime.Today, "Proposta Teste", 100m, "À vista", "30 dias", "", "",
            DateTime.Today, DateTime.Today.AddDays(30), "Aberto");

        var result = await handler.Handle(command);

        AssertInternalServerError(result);
    }

    [TestMethod]
    public async Task UsuarioHandler_ExcecaoSemInnerException_DeveRetornarErroInterno()
    {
        var tokenUtils = new TokenUtils(new FakeConfigurationService());
        var emailService = new EmailService("", 587, "", "", "", "", true);
        var handler = new UsuarioHandler(
            new ThrowingUsuarioRepository(),
            new FakeAutorizacaoRepository(),
            tokenUtils,
            emailService,
            new FakeUnitOfWork(),
            NullLogger<UsuarioHandler>.Instance);
        var command = new CreateUsuarioCommand("Usuário Teste", "usuario@teste.com", "123");

        var result = await handler.Handle(command);

        AssertInternalServerError(result);
    }

    private static void AssertInternalServerError(GenericResult result)
    {
        Assert.AreEqual(500, result.StatusCode);
        Assert.IsTrue(result.Errors.Count > 0);
        CollectionAssert.DoesNotContain(result.Errors, "detalhe interno");
    }
}
