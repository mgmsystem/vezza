using Vezza.Domain.CQRS.Commands.Usuarios;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;
using Vezza.Shared.Security.Password;
using Vezza.Shared.Security.Token;

namespace Vezza.Domain.CQRS.Handlers;
public class UsuarioHandler :
        IHandler<CreateUsuarioCommand>,
        IHandler<DeleteUsuarioCommand>
{
    private const int MaxTentativasCodigo = 5;
    private const string MensagemRecuperacao =
        "Se o e-mail estiver cadastrado e o serviço estiver disponível, as instruções serão enviadas.";
    private readonly IUsuarioRepository _repository;
    private readonly IAutorizacaoRepository _autorizacaoRepository;
    private readonly TokenUtils _tokenUtils;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsuarioHandler> _logger;
    private readonly IDemoModeService _demoMode;

    public UsuarioHandler(IUsuarioRepository repository, 
                            IAutorizacaoRepository autorizacaoRepository, 
                            TokenUtils tokenUtils,
                            IEmailService emailService,
                            IUnitOfWork unitOfWork,
                            ILogger<UsuarioHandler> logger,
                            IDemoModeService demoMode = null)
    {
        _repository = repository;
        _autorizacaoRepository = autorizacaoRepository;
        _tokenUtils = tokenUtils;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _demoMode = demoMode;
    }

    public async Task<GenericResult> Handle(CreateUsuarioCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var existsUsuario = await _repository.GetByEmail(command.Email);
            if(existsUsuario != null)
                return new GenericResult(400, "Email já cadastrado");

            var usuario = UsuarioService.CreateUsuarioFromCommand(command);

            await _repository.Save(usuario);

            usuario.Senha = null;

            return new GenericResult(201, usuario, $"v1/usuarios/{usuario.UsuarioId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateUsuarioCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var usuario = await _repository.GetById(command.UsuarioId);

            if (usuario == null)
                return GenericResult.NotFound();

            if (_demoMode?.IsDemoEmail(usuario.Email) == true)
                return DemoAccountForbidden();

            usuario = Services.UsuarioService.UpdataUsuarioFromCommand(command, usuario);            

            await _repository.Update(usuario);

            usuario.Senha = null;

            return new GenericResult(200, usuario);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteUsuarioCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var data = await _repository.GetById(command.UsuarioId);

            if (data == null)
                return GenericResult.NotFound();

            if (_demoMode?.IsDemoEmail(data.Email) == true)
                return DemoAccountForbidden();

            data = await _repository.Delete(data);

            data.Senha = null;

            return new GenericResult(200, data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }






    public async Task<GenericResult> Handle(RegistrarCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var existsUsuario = await _repository.GetByEmail(command.Email);
            if (existsUsuario != null)
                return new GenericResult(400, "Email já cadastrado");

            var usuario = UsuarioService.CreateUsuarioFromCommand(command);

            await _repository.Save(usuario);

            usuario.Senha = null;

            return new GenericResult(201, usuario, $"v1/usuarios/{usuario.UsuarioId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(AutenticarCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var usuario = await _repository.GetByEmail(command.Email);

            if (usuario == null)
                return new GenericResult(401, "Email ou senha inválidos!");

            if (!PasswordUtils.PasswordValid(usuario.Senha, command.Senha))
                return new GenericResult(401, "Email ou senha inválidos!");

            var token = _tokenUtils.GenerateToken(usuario.Nome, usuario.Email);

            return new GenericResult(200, new { Token = token, Type = "Bearer" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(AlterarSenhaCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            if (_demoMode?.IsDemoEmail(command.Email) == true)
                return DemoAccountForbidden();

            await _unitOfWork.BeginTransaction();

            var usuario = await _repository.GetByEmail(command.Email);

            if (usuario == null)
            {
                await _unitOfWork.RollbackTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            var autorizacao = await _autorizacaoRepository.GetLastByEmailForUpdate(command.Email, "TrocarSenha");

            if (autorizacao == null)
            {
                await _unitOfWork.RollbackTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            if (autorizacao.Utilizado)
            {
                await _unitOfWork.RollbackTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            if (DateTime.UtcNow > autorizacao.Validade)
            {
                await _unitOfWork.RollbackTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            if (autorizacao.Tentativas >= MaxTentativasCodigo)
            {
                await _unitOfWork.RollbackTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            if (!_tokenUtils.CodigoAutorizacaoValido(command.CodigoEmail, autorizacao.Codigo))
            {
                autorizacao.Tentativas++;
                await _autorizacaoRepository.Update(autorizacao);
                await _unitOfWork.CommitTransaction();
                return new GenericResult(401, "Código de recuperação inválido!");
            }

            usuario.Senha = PasswordUtils.PasswordEncripty(command.Senha);
            usuario = await _repository.Update(usuario);

            autorizacao.Utilizado = true;
            autorizacao = await _autorizacaoRepository.Update(autorizacao);            

            await _unitOfWork.CommitTransaction();

            return new GenericResult(200, "");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(CreateAutorizacaoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            if (_demoMode?.IsDemoEmail(command.Email) == true)
                return DemoAccountForbidden();

            var usuario = await _repository.GetByEmail(command.Email);

            if (usuario == null)
                return new GenericResult(200, (object)MensagemRecuperacao);

            var codigo = _tokenUtils.GerarCodigoAutorizacao();
            var autorizacao = new Autorizacao();
            autorizacao.Email = command.Email;
            autorizacao.Codigo = _tokenUtils.HashCodigoAutorizacao(codigo);
            autorizacao.Tipo = "TrocarSenha";
            autorizacao.Validade = DateTime.UtcNow.AddMinutes(5);
            autorizacao.Utilizado = false;
            autorizacao.Tentativas = 0;

            await _autorizacaoRepository.Save(autorizacao);

            var enviado = await _emailService.SendEmailAsync(
                command.Email,
                "Vezza - Trocar senha",
                _emailService.TemplateCodigoAutorizacao(codigo));

            if (!enviado)
            {
                // Um código que não chegou ao usuário não deve permanecer utilizável.
                autorizacao.Utilizado = true;
                await _autorizacaoRepository.Update(autorizacao);
            }

            return new GenericResult(200, (object)MensagemRecuperacao);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(UsuarioHandler));
            return GenericResult.InternalServerError();
        }
    }

    private static GenericResult DemoAccountForbidden() =>
        new(403, "A conta de demonstração não permite gerenciamento ou alteração de senha.");
}
