using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;
using System;

namespace Vezza.Domain.CQRS.Commands.Fluxos;

public class UpdateFluxoCommand : Validable, ICommand
{
    public UpdateFluxoCommand(int fluxoId, int orcamentoId, DateTime data, string descricao, string debitoCredito, decimal valor, string categoria)
    {
        Required()
            .IsRequired(fluxoId, "Id do fluxo é obrigatório")
            .IsRequired(orcamentoId, "Id do orçamento é obrigatório")
            .IsRequired(data, "Data é obrigatória")
            .IsNotNullOrEmpty(descricao, "Descrição é obrigatória")
            .IsNotNullOrEmpty(debitoCredito, "Indicativo de débito ou crédito é obrigatório")
            .IsInList(debitoCredito, new List<string> { "d", "c" }, "Valores possíveis para indicar Débito ou Crédito são: d ou c")
            .IsRequired(valor, "Valor é obrigatório")
            .IsNotNullOrEmpty(categoria, "Categoria é obrigatória");

        FluxoId = fluxoId;
        OrcamentoId = orcamentoId;
        Data = data;
        Descricao = descricao;
        DebitoCredito = debitoCredito;
        Valor = valor;
        Categoria = categoria;
    }

    public int FluxoId { get; set; }
    public int OrcamentoId { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; }
    public string DebitoCredito { get; set; }
    public decimal Valor { get; set; }
    public string Categoria { get; set; }
}
