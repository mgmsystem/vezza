using Vezza.Domain.CQRS.Commands.Clientes;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Domain.CQRS.Handlers;
public class ClienteHandler :
        IHandler<CreateClienteCommand>,
        IHandler<DeleteClienteCommand>
{
    private readonly IClienteRepository _repository;
    private readonly IObraRepository _obraRepository;
    private readonly ILogger<ClienteHandler> _logger;

    public ClienteHandler(
        IClienteRepository repository,
        IObraRepository obraRepository,
        ILogger<ClienteHandler> logger)
    {
        _repository = repository;
        _obraRepository = obraRepository;
        _logger = logger;
    }

    public async Task<GenericResult> Handle(CreateClienteCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var cliente = ClienteService.CreateClienteFromCommand(command);

            await _repository.Save(cliente);

            return new GenericResult(201, cliente, $"v1/clientes/{cliente.ClienteId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ClienteHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateClienteCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var cliente = await _repository.GetById(command.ClienteId);

            if (cliente == null)
                return GenericResult.NotFound();

            cliente = ClienteService.UpdateClienteFromCommand(command, cliente);

            await _repository.Update(cliente);

            return new GenericResult(200, cliente);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ClienteHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteClienteCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            if(await ClienteService.HasDependencies(command, _obraRepository))
                return new GenericResult(400, command.Errors());

            var cliente = await _repository.GetById(command.ClienteId);

            if (cliente == null)
                return GenericResult.NotFound();

            cliente = await _repository.Delete(cliente);

            return new GenericResult(200, cliente);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ClienteHandler));
            return GenericResult.InternalServerError();
        }
    }
}
