using Vezza.Domain.CQRS.Commands.Fluxos;
using Vezza.Domain.Entities;

namespace Vezza.Domain.Services;

public static class FluxoService
{
    public static Fluxo CreateFluxoFromCommand(CreateFluxoCommand command)
    {
        var fluxo = new Fluxo
        {
            OrcamentoId = command.OrcamentoId,
            Data = command.Data,
            Descricao = command.Descricao,
            DebitoCredito = command.DebitoCredito,
            Valor = command.Valor,
            Categoria = command.Categoria
        };

        return fluxo;
    }

    public static Fluxo UpdateFluxoFromCommand(UpdateFluxoCommand command, Fluxo fluxo)
    {
        fluxo.OrcamentoId = command.OrcamentoId;
        fluxo.Data = command.Data;
        fluxo.Descricao = command.Descricao;
        fluxo.DebitoCredito = command.DebitoCredito;
        fluxo.Valor = command.Valor;
        fluxo.Categoria = command.Categoria;

        return fluxo;
    }
}
