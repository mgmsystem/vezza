using Vezza.Domain.CQRS.Commands.OrcamentosItem;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Domain.CQRS.Handlers;
public class OrcamentoItemHandler :
        IHandler<CreateOrcamentoItemCommand>,
        IHandler<UpdateOrcamentoItemCommand>,
        IHandler<DeleteOrcamentoItemCommand>
{
    private readonly IOrcamentoItemRepository _repository;
    private readonly ILogger<OrcamentoItemHandler> _logger;

    public OrcamentoItemHandler(
        IOrcamentoItemRepository repository,
        ILogger<OrcamentoItemHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GenericResult> Handle(CreateOrcamentoItemCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var orcamentoItem = OrcamentoItemService.CreateOrcamentoItemFromCommand(command);

            await _repository.Save(orcamentoItem);

            return new GenericResult(201, orcamentoItem, $"v1/orcamento-items/{orcamentoItem.OrcamentoItemId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoItemHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateOrcamentoItemCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var orcamentoItem = await _repository.GetById(command.OrcamentoItemId);

            if (orcamentoItem == null)
                return GenericResult.NotFound();

            orcamentoItem = OrcamentoItemService.UpdateOrcamentoItemFromCommand(command, orcamentoItem);

            await _repository.Update(orcamentoItem);

            return new GenericResult(200, orcamentoItem);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoItemHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteOrcamentoItemCommand command)
    {
        try
        {
            if (command == null || !command.IsValid())
                return GenericResult.BadRequest();

            var orcamentoItem = await _repository.GetById(command.OrcamentoItemId);

            if (orcamentoItem == null)
                return GenericResult.NotFound();

            orcamentoItem = await _repository.Delete(orcamentoItem);

            return new GenericResult(200, orcamentoItem);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoItemHandler));
            return GenericResult.InternalServerError();
        }
    }
}
