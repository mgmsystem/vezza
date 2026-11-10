using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Api.Services;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Entities;
using Vezza.Domain.Services;

namespace Vezza.Tests.Integration;

[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class DemoModeConcurrencyIntegrationTests : MySqlIntegrationTestBase
{
    [TestMethod]
    public async Task DuasInclusoesSimultaneas_ComQuatroClientes_DevemFinalizarComCinco()
    {
        await using (var setup = CreateContext())
        {
            for (var index = 1; index <= 4; index++)
                setup.Clientes.Add(new Cliente($"Cliente {index}", $"DOC-{index}", null, null, null, null, null, null, null, null));
            await setup.SaveChangesAsync();
        }

        await using var contextA = CreateContext();
        await using var contextB = CreateContext();
        var serviceA = CreateService(contextA);
        var serviceB = CreateService(contextB);

        var results = await Task.WhenAll(
            CreateCliente(serviceA, contextA, "Concorrente A", "DOC-A"),
            CreateCliente(serviceB, contextB, "Concorrente B", "DOC-B"));

        CollectionAssert.AreEquivalent(new[] { 201, 409 }, results.Select(x => x.StatusCode).ToArray());
        Assert.AreEqual(DemoModeService.LimitMessage, results.Single(x => x.StatusCode == 409).Errors.Single());

        await using var verification = CreateContext();
        Assert.AreEqual(5, await verification.Clientes.CountAsync());
    }

    [TestMethod]
    public async Task Exclusao_DeveLiberarEspacoParaNovaInclusaoDaDemo()
    {
        await using var context = CreateContext();
        for (var index = 1; index <= 5; index++)
            context.Clientes.Add(new Cliente($"Cliente {index}", $"DOC-{index}", null, null, null, null, null, null, null, null));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var blocked = await CreateCliente(service, context, "Bloqueado", "DOC-X");
        Assert.AreEqual(409, blocked.StatusCode);

        var first = await context.Clientes.OrderBy(x => x.ClienteId).FirstAsync();
        context.Clientes.Remove(first);
        await context.SaveChangesAsync();

        var created = await CreateCliente(service, context, "Liberado", "DOC-Y");
        Assert.AreEqual(201, created.StatusCode);
        Assert.AreEqual(5, await context.Clientes.CountAsync());
    }

    [TestMethod]
    public async Task UsuarioComum_NaoDeveSofrerLimiteDaContaDemo()
    {
        await using var context = CreateContext();
        for (var index = 1; index <= 5; index++)
            context.Clientes.Add(new Cliente($"Cliente {index}", $"DOC-{index}", null, null, null, null, null, null, null, null));
        await context.SaveChangesAsync();

        var service = CreateService(context, "proprietario@vezza.app");
        var result = await CreateCliente(service, context, "Sexto cliente", "DOC-6");

        Assert.AreEqual(201, result.StatusCode);
        Assert.AreEqual(6, await context.Clientes.CountAsync());
    }

    private static DemoModeService CreateService(
        Vezza.Infra.Context.DataContext context,
        string currentEmail = "demo@vezza.app")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["DemoMode:Enabled"] = "true",
                ["DemoMode:Email"] = "demo@vezza.app"
            })
            .Build();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, currentEmail) },
                "Test"))
        };

        return new DemoModeService(
            context,
            new HttpContextAccessor { HttpContext = httpContext },
            configuration);
    }

    private static Task<GenericResult> CreateCliente(
        DemoModeService service,
        Vezza.Infra.Context.DataContext context,
        string name,
        string document)
    {
        return service.ExecuteCreateAsync(DemoResource.Cliente, async () =>
        {
            var cliente = new Cliente(name, document, null, null, null, null, null, null, null, null);
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return new GenericResult(201, cliente);
        });
    }
}
