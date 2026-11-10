using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Entities;
using Vezza.Infra.Context;
using Vezza.Infra.Repositories;
using Vezza.Shared.Security.Password;
using Vezza.Shared.Security.Token;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Integration;

[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class PasswordRecoveryConcurrencyIntegrationTests : MySqlIntegrationTestBase
{
    private const string Email = "concorrencia@teste.local";
    private const string Codigo = "123456";

    [TestMethod]
    public async Task AlterarSenha_ComCodigoValido_DeveConsumirAutorizacaoNoMySql()
    {
        await SeedRecoveryData();
        await using var context = CreateContext();
        var handler = CreateHandler(context);

        var result = await handler.Handle(new AlterarSenhaCommand(Email, "senha-nova", Codigo));

        var autorizacao = await context.Autorizacoes.AsNoTracking().SingleAsync();
        var usuario = await context.Usuarios.AsNoTracking().SingleAsync();
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(autorizacao.Utilizado);
        Assert.IsTrue(PasswordUtils.PasswordValid(usuario.Senha, "senha-nova"));
    }

    [TestMethod]
    public async Task AlterarSenha_ComDuasConexoesConcorrentes_DevePermitirUmUnicoConsumo()
    {
        await SeedRecoveryData();
        await using var contextA = CreateContext();
        await using var contextB = CreateContext();
        var handlerA = CreateHandler(contextA);
        var handlerB = CreateHandler(contextB);

        var results = await Task.WhenAll(
            handlerA.Handle(new AlterarSenhaCommand(Email, "senha-a", Codigo)),
            handlerB.Handle(new AlterarSenhaCommand(Email, "senha-b", Codigo)));

        Assert.AreEqual(1, results.Count(x => x.StatusCode == 200));
        Assert.AreEqual(1, results.Count(x => x.StatusCode == 401));
        await using var verificationContext = CreateContext();
        Assert.IsTrue((await verificationContext.Autorizacoes.AsNoTracking().SingleAsync()).Utilizado);
    }

    private async Task SeedRecoveryData()
    {
        var tokenUtils = new TokenUtils(new FakeConfigurationService());
        await using var context = CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Nome = "Usuário concorrência",
            Email = Email,
            Senha = PasswordUtils.PasswordEncripty("senha-antiga"),
            Nivel = 0
        });
        context.Autorizacoes.Add(new Autorizacao
        {
            Email = Email,
            Tipo = "TrocarSenha",
            Codigo = tokenUtils.HashCodigoAutorizacao(Codigo),
            Validade = DateTime.UtcNow.AddMinutes(5),
            Utilizado = false,
            Tentativas = 0
        });
        await context.SaveChangesAsync();
    }

    private static UsuarioHandler CreateHandler(DataContext context)
    {
        var tokenUtils = new TokenUtils(new FakeConfigurationService());
        return new UsuarioHandler(
            new UsuarioRepository(context),
            new AutorizacaoRepository(context),
            tokenUtils,
            new FakeEmailService(),
            new UnitOfWork(context),
            NullLogger<UsuarioHandler>.Instance);
    }
}
