using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.OrcamentosItem;

public class DeleteOrcamentoItemCommand : Validable, ICommand
{
    public DeleteOrcamentoItemCommand(int orcamentoItemId)
    {
        Required()
            .IsRequired(orcamentoItemId, "Id do item do orçamento é obrigatório");

        OrcamentoItemId = orcamentoItemId;
    }

    public int OrcamentoItemId { get; set; }
}
