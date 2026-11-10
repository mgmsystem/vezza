using Vezza.Domain.CQRS.Commands.OrcamentosItem;
using Vezza.Domain.Entities;

namespace Vezza.Domain.Services;

public static class OrcamentoItemService
{
    public static OrcamentoItem CreateOrcamentoItemFromCommand(CreateOrcamentoItemCommand command)
    {
        var orcamentoItem = new OrcamentoItem
        {
            OrcamentoId = command.OrcamentoId,
            Descricao = command.Descricao,
            Valor = command.Valor
        };

        return orcamentoItem;
    }

    public static OrcamentoItem UpdateOrcamentoItemFromCommand(UpdateOrcamentoItemCommand command, OrcamentoItem orcamentoItem)
    {
        orcamentoItem.OrcamentoId = command.OrcamentoId;
        orcamentoItem.Descricao = command.Descricao;
        orcamentoItem.Valor = command.Valor;

        return orcamentoItem;
    }
}
