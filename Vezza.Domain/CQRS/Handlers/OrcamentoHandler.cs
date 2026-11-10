using Vezza.Domain.CQRS.Commands.Orcamentos;
using Microsoft.Extensions.Logging;
using Vezza.Domain.CQRS.Handlers.Interfaces;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;
using Vezza.Domain.Services;

namespace Vezza.Domain.CQRS.Handlers;
public class OrcamentoHandler :
        IHandler<CreateOrcamentoCommand>,
        IHandler<UpdateOrcamentoCommand>,
        IHandler<DeleteOrcamentoCommand>
{
    private readonly IOrcamentoRepository _repository;
    private readonly IOrcamentoItemRepository _orcamentoItemRepository;
    private readonly IFluxoRepository _fluxoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrcamentoHandler> _logger;

    public OrcamentoHandler(IOrcamentoRepository repository,
                                IOrcamentoItemRepository orcamentoItemRepository,
                                IFluxoRepository fluxoRepository,
                                IUnitOfWork unitOfWork,
                                ILogger<OrcamentoHandler> logger)
    {
        _repository = repository;
        _orcamentoItemRepository = orcamentoItemRepository;
        _fluxoRepository = fluxoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GenericResult> Handle(CreateOrcamentoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var orcamento = OrcamentoService.CreateOrcamentoFromCommand(command);

            await _repository.Save(orcamento);

            return new GenericResult(201, orcamento, $"v1/orcamentos/{orcamento.OrcamentoId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(UpdateOrcamentoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            var orcamento = await _repository.GetById(command.OrcamentoId);

            if (orcamento == null)
                return GenericResult.NotFound();

            orcamento = OrcamentoService.UpdateOrcamentoFromCommand(command, orcamento);

            await _repository.Update(orcamento);

            return new GenericResult(200, orcamento);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoHandler));
            return GenericResult.InternalServerError();
        }
    }

    public async Task<GenericResult> Handle(DeleteOrcamentoCommand command)
    {
        try
        {
            if (command == null)
                return GenericResult.BadRequest();

            if (!command.IsValid())
                return new GenericResult(400, command.Errors());

            await _unitOfWork.BeginTransaction();

            var orcamento = await _repository.GetById(command.OrcamentoId);

            if (orcamento == null)
            {
                await _unitOfWork.RollbackTransaction();
                return GenericResult.NotFound();
            }

            await OrcamentoService.ClearDependencies(command, _orcamentoItemRepository, _fluxoRepository);

            orcamento = await _repository.Delete(orcamento);

            await _unitOfWork.CommitTransaction();

            return new GenericResult(200, orcamento);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(e, "Falha interna ao processar operação no {Handler}.", nameof(OrcamentoHandler));
            return GenericResult.InternalServerError();
        }
    }
}
