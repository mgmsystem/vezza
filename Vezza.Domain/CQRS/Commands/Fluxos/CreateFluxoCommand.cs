using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;
using System;

namespace Vezza.Domain.CQRS.Commands.Fluxos;

public class CreateFluxoCommand : Validable, ICommand
{
    public CreateFluxoCommand(int orcamentoId, DateTime data, string descricao, string debitoCredito, decimal valor, string categoria)
    {
        Required()
            .IsRequired(orcamentoId, "Id do orçamento é obrigatório")
            .IsRequired(data, "Data é obrigatória")
            .IsNotNullOrEmpty(descricao, "Descrição é obrigatória")
            .IsNotNullOrEmpty(debitoCredito, "Indicativo de débito ou crédito é obrigatório")
            .IsInList(debitoCredito, new List<string> { "d", "c" }, "Valores possíveis para indicar Débito ou Crédito são: d ou c")
            .IsRequired(valor, "Valor é obrigatório")
            .IsNotNullOrEmpty(categoria, "Categoria é obrigatória");

        OrcamentoId = orcamentoId;
        Data = data;
        Descricao = descricao;
        DebitoCredito = debitoCredito;
        Valor = valor;
        Categoria = categoria;
    }

    public int OrcamentoId { get; }
    public DateTime Data { get; }
    public string Descricao { get; }
    public string DebitoCredito { get; }
    public decimal Valor { get; }
    public string Categoria { get; }
}
