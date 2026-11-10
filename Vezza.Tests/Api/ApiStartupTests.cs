#nullable enable

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Repositories;
using Vezza.Infra.Context;

namespace Vezza.Tests.Api;

[TestClass]
public class ApiStartupTests
{
    [TestMethod]
    public async Task Host_ComConfiguracaoFicticia_DeveConstruirContainerEResolverHandlers()
    {
        using var factory = new VezzaApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        using var scope = factory.Services.CreateScope();

        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        scope.ServiceProvider.GetRequiredService<UsuarioHandler>();
        scope.ServiceProvider.GetRequiredService<ClienteHandler>();
        scope.ServiceProvider.GetRequiredService<ObraHandler>();
        scope.ServiceProvider.GetRequiredService<OrcamentoHandler>();
        scope.ServiceProvider.GetRequiredService<OrcamentoItemHandler>();
        scope.ServiceProvider.GetRequiredService<FluxoHandler>();

        Assert.IsNotNull(dataContext);
        Assert.IsInstanceOfType<UnitOfWork>(unitOfWork);
        Assert.AreSame(dataContext, scope.ServiceProvider.GetRequiredService<DataContext>());
        Assert.AreSame(unitOfWork, scope.ServiceProvider.GetRequiredService<IUnitOfWork>());

        var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.AreEqual(HttpStatusCode.OK, swaggerResponse.StatusCode);
    }

    [TestMethod]
    public async Task Swagger_EmProduction_NaoDeveSerExpostoAutomaticamente()
    {
        using var factory = new VezzaApiFactory("Production");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var swaggerJson = await client.GetAsync("/swagger/v1/swagger.json");
        var swaggerUi = await client.GetAsync("/swagger/index.html");

        Assert.AreEqual(HttpStatusCode.NotFound, swaggerJson.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, swaggerUi.StatusCode);
    }

    private sealed class VezzaApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _environment;

        public VezzaApiFactory(string environment = "Development")
        {
            _environment = environment;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:connectionString",
                "Server=127.0.0.1;Port=33070;Database=vezza_fake;User=fake;Password=fake;SslMode=Disabled");
            builder.UseSetting(
                "JwtKey",
                "chave-ficticia-exclusiva-para-teste-de-inicializacao-vezza");
            builder.UseEnvironment(_environment);
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
            builder.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:connectionString"] =
                        "Server=127.0.0.1;Port=33070;Database=vezza_fake;User=fake;Password=fake;SslMode=Disabled",
                    ["JwtKey"] = "chave-ficticia-exclusiva-para-teste-de-inicializacao-vezza"
                });
            });
        }
    }
}
