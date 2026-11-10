using Vezza.Domain.CQRS.Commands.Fluxos;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Domain.CQRS.Handlers;

public class FluxoHandler :
        IHandler<CreateFluxoCommand>,
        IHandler<UpdateFluxoCommand>,
        IHandler<DeleteFluxoCommand>
{
    private readonly IFluxoRepository _repository;
    private readonly ILogger<FluxoHandler> _logger;

    public FluxoHandler(IFluxoRepository repository, ILogger<FluxoHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GenericResult> Handle(CreateFluxoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var fluxo = FluxoService.CreateFluxoFromCommand(command);

            await _repository.Save(fluxo);

            return new GenericResult(201, fluxo, $"v1/fluxos/{fluxo.FluxoId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(FluxoHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateFluxoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var fluxo = await _repository.GetById(command.FluxoId);

            if (fluxo == null)
                return GenericResult.NotFound();

            fluxo = FluxoService.UpdateFluxoFromCommand(command, fluxo);

            await _repository.Update(fluxo);

            return new GenericResult(200, fluxo);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(FluxoHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteFluxoCommand command)
    {
        try
        {
            if (command == null || !command.IsValid())
                return GenericResult.BadRequest();

            var fluxo = await _repository.GetById(command.FluxoId);

            if (fluxo == null)
                return GenericResult.NotFound();

            await _repository.Delete(fluxo);

            return new GenericResult(200, null, "Fluxo deletado com sucesso.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(FluxoHandler));
            return GenericResult.InternalServerError();
        }
    }
}
