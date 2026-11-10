using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Vezza.Api.Controllers;
using Vezza.Domain.CQRS.Results;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Controllers;

[TestClass]
public class ControllerContractTests
{
    [TestMethod]
    public async Task GetClientePorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var repository = new FakeClienteRepository { Cliente = null };
        var result = await new ClienteController().GetByIdAsync(repository, 999);

        AssertNotFound(result);
    }

    [TestMethod]
    public async Task GetObraPorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var repository = new FakeObraRepository { Obra = null };
        var result = await new ObraController().GetByIdAsync(repository, 999);

        AssertNotFound(result);
    }

    [TestMethod]
    public async Task GetOrcamentoPorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var repository = new FakeOrcamentoRepository { Orcamento = null };
        var result = await new OrcamentoController().GetByIdAsync(repository, 999);

        AssertNotFound(result);
    }

    [TestMethod]
    public async Task GetOrcamentoItemPorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var result = await new OrcamentoItemController()
            .GetByIdAsync(new FakeOrcamentoItemRepository(), 999);

        AssertNotFound(result);
    }

    [TestMethod]
    public async Task GetFluxoPorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var result = await new FluxoController().GetByIdAsync(new FakeFluxoRepository(), 999);

        AssertNotFound(result);
    }

    [TestMethod]
    public async Task GetUsuarioPorId_QuandoInexistente_DeveRetornarNotFound()
    {
        var repository = new FakeUsuarioSecurityRepository { Usuario = null };
        var result = await new UsuarioController().GetByIdAsync(repository, 999);

        AssertNotFound(result);
    }

    private static void AssertNotFound(IActionResult actionResult)
    {
        var objectResult = actionResult as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(404, objectResult.StatusCode);

        var result = objectResult.Value as GenericResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.IsNull(result.Data);
    }
}
