using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Vezza.Domain.CQRS.Commands.Clientes;
using Vezza.Domain.CQRS.Commands.Fluxos;
using Vezza.Domain.CQRS.Commands.Obras;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Commands.OrcamentosItem;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Services;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Handlers;

[TestClass]
public class DemoModeHandlerTests
{
    [TestMethod]
    public async Task InclusoesDaDemo_DevemAplicarLimiteEmTodasAsCategorias()
    {
        var demo = new FakeDemoModeService { IsDemoRequest = true, BlockCreates = true };

        await AssertLimited(
            await new ClienteHandler(new FakeClienteRepository(), new FakeObraRepository(),
                NullLogger<ClienteHandler>.Instance, demo)
                .Handle(new CreateClienteCommand("Cliente demo", "123", null, null, null, null, null, null, null, null)),
            demo, DemoResource.Cliente);

        await AssertLimited(
            await new ObraHandler(new FakeObraRepository(), new FakeOrcamentoRepository(),
                NullLogger<ObraHandler>.Instance, demo)
                .Handle(new CreateObraCommand(1, "Obra demo", null, null, null, null, null, null, null)),
            demo, DemoResource.Obra);

        await AssertLimited(
            await new OrcamentoHandler(new FakeOrcamentoRepository(), new FakeOrcamentoItemRepository(),
                new FakeFluxoRepository(), new FakeUnitOfWork(), NullLogger<OrcamentoHandler>.Instance, demo)
                .Handle(new CreateOrcamentoCommand(1, DateTime.UtcNow, "Proposta demo", 10m,
                    null, null, null, null, default, default, "Aberto")),
            demo, DemoResource.Orcamento);

        await AssertLimited(
            await new OrcamentoItemHandler(new FakeOrcamentoItemRepository(),
                NullLogger<OrcamentoItemHandler>.Instance, demo)
                .Handle(new CreateOrcamentoItemCommand(1, "Item demo", 10m)),
            demo, DemoResource.OrcamentoItem);

        await AssertLimited(
            await new FluxoHandler(new FakeFluxoRepository(), NullLogger<FluxoHandler>.Instance, demo)
                .Handle(new CreateFluxoCommand(1, DateTime.UtcNow, "Fluxo demo", "c", 10m, "Teste")),
            demo, DemoResource.Fluxo);
    }

    private static Task AssertLimited(
        Vezza.Domain.CQRS.Results.GenericResult result,
        FakeDemoModeService demo,
        DemoResource expectedResource)
    {
        Assert.AreEqual(409, result.StatusCode);
        CollectionAssert.Contains(result.Errors, Vezza.Api.Services.DemoModeService.LimitMessage);
        Assert.AreEqual(expectedResource, demo.LastResource);
        return Task.CompletedTask;
    }
}
