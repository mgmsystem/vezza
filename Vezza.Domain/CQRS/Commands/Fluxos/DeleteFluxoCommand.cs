using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Fluxos;

public class DeleteFluxoCommand : Validable, ICommand
{
    public DeleteFluxoCommand(int fluxoId)
    {
        Required()
            .IsRequired(fluxoId, "Id do fluxo é obrigatório");

        FluxoId = fluxoId;
    }

    public int FluxoId { get; set; }
}
