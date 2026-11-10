using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Entities;
using Vezza.Shared.Security.Password;
using Vezza.Shared.Security.Token;
using Vezza.Tests.Repositories;

namespace Vezza.Tests.Handlers;

[TestClass]
public class AuthenticationSecurityTests
{
    private const string Email = "usuario@teste.com";

    [TestMethod]
    public async Task Autenticar_ComCredenciaisValidas_DeveEmitirTokenSemExporSenha()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-correta"));

        var result = await dependencies.Handler.Handle(
            new AutenticarCommand(Email, "senha-correta"));

        Assert.AreEqual(200, result.StatusCode);
        var token = GetToken(result.Data);
        Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        Assert.IsNull(result.Data.GetType().GetProperty("Senha"));
        Assert.IsFalse(result.Data.ToString().Contains(dependencies.UsuarioRepository.Usuario.Senha));
    }

    [TestMethod]
    public async Task Autenticar_ComSenhaIncorreta_DeveRetornarUnauthorized()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-correta"));

        var result = await dependencies.Handler.Handle(
            new AutenticarCommand(Email, "senha-incorreta"));

        Assert.AreEqual(401, result.StatusCode);
    }

    [TestMethod]
    public async Task Autenticar_ComUsuarioInexistente_DeveRetornarUnauthorized()
    {
        var dependencies = CreateDependencies(null);

        var result = await dependencies.Handler.Handle(
            new AutenticarCommand(Email, "senha-correta"));

        Assert.AreEqual(401, result.StatusCode);
    }

    [TestMethod]
    public async Task TokenEmitido_DeveSerValidadoComAMesmaChave()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-correta"));
        var result = await dependencies.Handler.Handle(
            new AutenticarCommand(Email, "senha-correta"));
        var token = GetToken(result.Data);
        var key = Encoding.ASCII.GetBytes(dependencies.Configuration.GetJwtKey());
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        Assert.AreEqual(Email, principal.FindFirst(ClaimTypes.Email)?.Value);
    }

    [TestMethod]
    public async Task AlterarSenha_ComCodigoValido_DeveConsumirCodigoEAtualizarSenha()
    {
        const string codigo = "123456";
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.AutorizacaoRepository.Autorizacao = CreateAutorizacao(
            dependencies.TokenUtils.HashCodigoAutorizacao(codigo),
            DateTime.UtcNow.AddMinutes(5));

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(Email, "senha-nova", codigo));

        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dependencies.AutorizacaoRepository.Autorizacao.Utilizado);
        Assert.IsTrue(dependencies.UsuarioRepository.UpdateCalled);
        Assert.IsTrue(PasswordUtils.PasswordValid(
            dependencies.UsuarioRepository.Usuario.Senha,
            "senha-nova"));
    }

    [TestMethod]
    public async Task AlterarSenha_ComCodigoExpirado_DeveRejeitar()
    {
        const string codigo = "123456";
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.AutorizacaoRepository.Autorizacao = CreateAutorizacao(
            dependencies.TokenUtils.HashCodigoAutorizacao(codigo),
            DateTime.UtcNow.AddMinutes(-1));

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(Email, "senha-nova", codigo));

        Assert.AreEqual(401, result.StatusCode);
        Assert.IsFalse(dependencies.UsuarioRepository.UpdateCalled);
    }

    [TestMethod]
    public async Task AlterarSenha_ComCodigoUtilizado_DeveRejeitarReutilizacao()
    {
        const string codigo = "123456";
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.AutorizacaoRepository.Autorizacao = CreateAutorizacao(
            dependencies.TokenUtils.HashCodigoAutorizacao(codigo),
            DateTime.UtcNow.AddMinutes(5),
            utilizado: true);

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(Email, "senha-nova", codigo));

        Assert.AreEqual(401, result.StatusCode);
        Assert.IsFalse(dependencies.UsuarioRepository.UpdateCalled);
    }

    [TestMethod]
    public async Task AlterarSenha_AposExcessoDeTentativas_DeveRejeitarMesmoCodigoValido()
    {
        const string codigo = "123456";
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.AutorizacaoRepository.Autorizacao = CreateAutorizacao(
            dependencies.TokenUtils.HashCodigoAutorizacao(codigo),
            DateTime.UtcNow.AddMinutes(5),
            tentativas: 5);

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(Email, "senha-nova", codigo));

        Assert.AreEqual(401, result.StatusCode);
        Assert.IsFalse(dependencies.UsuarioRepository.UpdateCalled);
    }

    [TestMethod]
    public async Task AlterarSenha_ComCodigoIncorreto_DevePersistirTentativa()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.AutorizacaoRepository.Autorizacao = CreateAutorizacao(
            dependencies.TokenUtils.HashCodigoAutorizacao("123456"),
            DateTime.UtcNow.AddMinutes(5));

        var result = await dependencies.Handler.Handle(
            new AlterarSenhaCommand(Email, "senha-nova", "654321"));

        Assert.AreEqual(401, result.StatusCode);
        Assert.AreEqual(1, dependencies.AutorizacaoRepository.Autorizacao.Tentativas);
        Assert.AreEqual(1, dependencies.AutorizacaoRepository.UpdateCount);
    }

    [TestMethod]
    public async Task SolicitarRecuperacao_DeveArmazenarHashSemExporCodigoNaResposta()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));

        var result = await dependencies.Handler.Handle(new CreateAutorizacaoCommand(Email));

        var codigoEnviado = Regex.Match(dependencies.EmailService.LastBody, @"<b>(\d{6})</b>").Groups[1].Value;
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(6, codigoEnviado.Length);
        Assert.AreNotEqual(codigoEnviado, dependencies.AutorizacaoRepository.Autorizacao.Codigo);
        Assert.AreEqual(64, dependencies.AutorizacaoRepository.Autorizacao.Codigo.Length);
        Assert.IsFalse(result.Data.ToString().Contains(codigoEnviado));
        Assert.IsFalse(result.Data.ToString().Contains(dependencies.AutorizacaoRepository.Autorizacao.Codigo));
    }

    [TestMethod]
    public async Task SolicitarRecuperacao_ComFalhaSmtp_DeveInvalidarCodigoERetornarMensagemNeutra()
    {
        var dependencies = CreateDependencies(CreateUsuario("senha-antiga"));
        dependencies.EmailService.SendResult = false;

        var result = await dependencies.Handler.Handle(new CreateAutorizacaoCommand(Email));

        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dependencies.AutorizacaoRepository.Autorizacao.Utilizado);
        StringAssert.Contains(result.Data.ToString(), "Se o e-mail estiver cadastrado");
    }

    [TestMethod]
    public async Task SolicitarRecuperacao_ComEmailInexistente_DeveRetornarMesmaMensagemNeutra()
    {
        var existente = CreateDependencies(CreateUsuario("senha-antiga"));
        var inexistente = CreateDependencies(null);

        var resultadoExistente = await existente.Handler.Handle(new CreateAutorizacaoCommand(Email));
        var resultadoInexistente = await inexistente.Handler.Handle(new CreateAutorizacaoCommand(Email));

        Assert.AreEqual(200, resultadoInexistente.StatusCode);
        Assert.AreEqual(resultadoExistente.Data, resultadoInexistente.Data);
        Assert.IsFalse(inexistente.EmailService.SendCalled);
    }

    private static SecurityDependencies CreateDependencies(Usuario usuario)
    {
        var configuration = new FakeConfigurationService();
        var tokenUtils = new TokenUtils(configuration);
        var usuarioRepository = new FakeUsuarioSecurityRepository { Usuario = usuario };
        var autorizacaoRepository = new FakeAutorizacaoSecurityRepository();
        var emailService = new FakeEmailService();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UsuarioHandler(
            usuarioRepository,
            autorizacaoRepository,
            tokenUtils,
            emailService,
            unitOfWork,
            NullLogger<UsuarioHandler>.Instance);

        return new SecurityDependencies(
            handler,
            usuarioRepository,
            autorizacaoRepository,
            emailService,
            tokenUtils,
            configuration,
            unitOfWork);
    }

    private static Usuario CreateUsuario(string senha)
    {
        return new Usuario
        {
            UsuarioId = 1,
            Nome = "Usuário Teste",
            Email = Email,
            Senha = PasswordUtils.PasswordEncripty(senha),
            Nivel = 0
        };
    }

    private static Autorizacao CreateAutorizacao(
        string codigo,
        DateTime validade,
        bool utilizado = false,
        int tentativas = 0)
    {
        return new Autorizacao
        {
            AutorizacaoId = 1,
            Email = Email,
            Tipo = "TrocarSenha",
            Codigo = codigo,
            Validade = validade,
            Utilizado = utilizado,
            Tentativas = tentativas
        };
    }

    private static string GetToken(object data)
    {
        return data.GetType().GetProperty("Token")?.GetValue(data)?.ToString();
    }

    private sealed class SecurityDependencies
    {
        public SecurityDependencies(
            UsuarioHandler handler,
            FakeUsuarioSecurityRepository usuarioRepository,
            FakeAutorizacaoSecurityRepository autorizacaoRepository,
            FakeEmailService emailService,
            TokenUtils tokenUtils,
            FakeConfigurationService configuration,
            FakeUnitOfWork unitOfWork)
        {
            Handler = handler;
            UsuarioRepository = usuarioRepository;
            AutorizacaoRepository = autorizacaoRepository;
            EmailService = emailService;
            TokenUtils = tokenUtils;
            Configuration = configuration;
            UnitOfWork = unitOfWork;
        }

        public UsuarioHandler Handler { get; }
        public FakeUsuarioSecurityRepository UsuarioRepository { get; }
        public FakeAutorizacaoSecurityRepository AutorizacaoRepository { get; }
        public FakeEmailService EmailService { get; }
        public TokenUtils TokenUtils { get; }
        public FakeConfigurationService Configuration { get; }
        public FakeUnitOfWork UnitOfWork { get; }
    }
}
