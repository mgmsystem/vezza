using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.OrcamentosItem;

public class UpdateOrcamentoItemCommand : Validable, ICommand
{
    public UpdateOrcamentoItemCommand(int orcamentoItemId, int orcamentoId, string descricao, decimal valor)
    {
        Required()
            .IsRequired(orcamentoItemId, "Id do item do orçamento é obrigatório")
            .IsRequired(orcamentoId, "Id do orçamento é obrigatório")
            .IsNotNullOrEmpty(descricao, "Descrição é obrigatória")
            .IsRequired(valor, "Valor é obrigatório");

        OrcamentoItemId = orcamentoItemId;
        OrcamentoId = orcamentoId;
        Descricao = descricao;
        Valor = valor;
    }

    public int OrcamentoItemId { get; set; }
    public int OrcamentoId { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
}
