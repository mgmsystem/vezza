using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Orcamentos;
public class DeleteOrcamentoCommand : Validable, ICommand
{
    public DeleteOrcamentoCommand(int orcamentoId)
    {
        Required()
            .IsRequired(orcamentoId, "Id do orçamento é obrigatório");

        OrcamentoId = orcamentoId;
    }

    public int OrcamentoId { get; set; }
}
