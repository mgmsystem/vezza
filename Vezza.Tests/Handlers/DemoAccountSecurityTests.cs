using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Vezza.Api.Controllers;
using Vezza.Api.Security;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Entities;
using Vezza.Shared.Security.Token;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Handlers;

[TestClass]
public class DemoAccountSecurityTests
{
    private const string DemoEmail = "demo@vezza.app";

    [TestMethod]
    public async Task Demo_NaoDeveAlterarSenhaNemSolicitarRecuperacao()
    {
        var dependencies = CreateHandler();

        var passwordResult = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(DemoEmail, "nova-senha", "123456"));
        var recoveryResult = await dependencies.Handler.Handle(
            new CreateAutorizacaoCommand(DemoEmail));

        Assert.AreEqual(403, passwordResult.StatusCode);
        Assert.AreEqual(403, recoveryResult.StatusCode);
        Assert.IsFalse(dependencies.EmailService.SendCalled);
        Assert.AreEqual(0, dependencies.UnitOfWork.BeginCount);
    }

    [TestMethod]
    public async Task ContaDemo_NaoDeveSerAlteradaNemExcluidaPorOutroUsuario()
    {
        var dependencies = CreateHandler();

        var updateResult = await dependencies.Handler.Handle(new UpdateUsuarioCommand(1, "Outro nome"));
        var deleteResult = await dependencies.Handler.Handle(new DeleteUsuarioCommand(1));

        Assert.AreEqual(403, updateResult.StatusCode);
        Assert.AreEqual(403, deleteResult.StatusCode);
        Assert.IsFalse(dependencies.UsuarioRepository.UpdateCalled);
    }

    [TestMethod]
    public async Task Middleware_DeveBloquearGerenciamentoDeUsuariosPelaDemo()
    {
        var nextCalled = false;
        var middleware = new DemoAccountProtectionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/usuarios";
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, DemoEmail) },
                "Test"));

        await middleware.InvokeAsync(
            context,
            new FakeDemoModeService { IsDemoRequest = true });

        Assert.AreEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.IsFalse(nextCalled);
    }

    [TestMethod]
    public async Task RegistroPublicoDesabilitado_DeveRetornarForbiddenSemCriarUsuario()
    {
        var controller = new UsuarioController();
        var result = await controller.RegistrarAsync(
            new RegistrarCommand("Primeiro usuário", "owner@vezza.app", "senha-segura"),
            null,
            new FakeUsuarioSecurityRepository { Usuario = null },
            new FakeDemoModeService { PublicRegistrationEnabled = false });

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(403, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task RegistroPublicoHabilitado_DeveSerBloqueadoQuandoJaExisteUsuario()
    {
        var controller = new UsuarioController();
        var result = await controller.RegistrarAsync(
            new RegistrarCommand("Outro usuário", "outro@vezza.app", "senha-segura"),
            null,
            new FakeUsuarioSecurityRepository { Usuario = new Usuario { Email = "owner@vezza.app" } },
            new FakeDemoModeService { PublicRegistrationEnabled = true });

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(403, objectResult.StatusCode);
    }

    private static Dependencies CreateHandler()
    {
        var usuarioRepository = new FakeUsuarioSecurityRepository
        {
            Usuario = new Usuario
            {
                UsuarioId = 1,
                Nome = "Demonstração",
                Email = DemoEmail,
                Senha = "hash",
                Nivel = 0
            }
        };
        var emailService = new FakeEmailService();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UsuarioHandler(
            usuarioRepository,
            new FakeAutorizacaoSecurityRepository(),
            new TokenUtils(new FakeConfigurationService()),
            emailService,
            unitOfWork,
            NullLogger<UsuarioHandler>.Instance,
            new FakeDemoModeService { IsDemoRequest = true });

        return new Dependencies(handler, usuarioRepository, emailService, unitOfWork);
    }

    private sealed record Dependencies(
        UsuarioHandler Handler,
        FakeUsuarioSecurityRepository UsuarioRepository,
        FakeEmailService EmailService,
        FakeUnitOfWork UnitOfWork);
}
