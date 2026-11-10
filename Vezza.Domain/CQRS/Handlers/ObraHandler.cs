using Vezza.Domain.CQRS.Commands.Obras;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Domain.CQRS.Handlers;
public class ObraHandler :
        IHandler<CreateObraCommand>,
        IHandler<UpdateObraCommand>,
        IHandler<DeleteObraCommand>
{
    private readonly IObraRepository _repository;
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly ILogger<ObraHandler> _logger;
    private readonly IDemoModeService _demoMode;

    public ObraHandler(
        IObraRepository repository,
        IOrcamentoRepository orcamentoRepository,
        ILogger<ObraHandler> logger,
        IDemoModeService demoMode = null)
    {
        _repository = repository;
        _orcamentoRepository = orcamentoRepository;
        _logger = logger;
        _demoMode = demoMode;
    }

    public async Task<GenericResult> Handle(CreateObraCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            async Task<GenericResult> Create()
            {
                var obra = ObraService.CreateObraFromCommand(command);
                await _repository.Save(obra);
                return new GenericResult(201, obra, $"v1/obras/{obra.ObraId}");
            }

            return _demoMode == null
                ? await Create()
                : await _demoMode.ExecuteCreateAsync(DemoResource.Obra, Create);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ObraHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateObraCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var obra = await _repository.GetById(command.ObraId);

            if (obra == null)
                return GenericResult.NotFound();

            obra = ObraService.UpdateObraFromCommand(command, obra);

            await _repository.Update(obra);

            return new GenericResult(200, obra);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ObraHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteObraCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            if (await ObraService.HasDependencies(command, _orcamentoRepository))
                return new GenericResult(400, command.Errors());

            var obra = await _repository.GetById(command.ObraId);

            if (obra == null)
                return GenericResult.NotFound();

            obra = await _repository.Delete(obra);

            return new GenericResult(200, obra);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(ObraHandler));
            return GenericResult.InternalServerError();
        }
    }
}
