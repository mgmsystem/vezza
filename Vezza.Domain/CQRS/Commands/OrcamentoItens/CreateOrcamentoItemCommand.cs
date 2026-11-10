using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.OrcamentosItem;

public class CreateOrcamentoItemCommand : Validable, ICommand
{
    public CreateOrcamentoItemCommand(int orcamentoId, string descricao, decimal valor)
    {
        Required()
            .IsRequired(orcamentoId, "Id do orçamento é obrigatório")
            .IsNotNullOrEmpty(descricao, "Descrição é obrigatória")
            .IsRequired(valor, "Valor é obrigatório");


        OrcamentoId = orcamentoId;
        Descricao = descricao;
        Valor = valor;
    }

    public int OrcamentoId { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
}
