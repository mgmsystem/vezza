using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;

namespace Vezza.Domain.Services;

public static class OrcamentoService
{
    public static Orcamento CreateOrcamentoFromCommand(CreateOrcamentoCommand command)
    {
        var orcamento = new Orcamento
        {
            ObraId = command.ObraId,
            Data = command.Data,
            ObjetoProposta = command.ObjetoProposta,
            Valor = command.Valor,
            CondPagamento = command.CondPagamento,
            Prazos = command.Prazos,
            Comentarios = command.Comentarios,
            NotasGerais = command.NotasGerais,
            InicioObra = command.InicioObra,
            TerminoObra = command.TerminoObra,
            Situacao = command.Situacao
        };

        return orcamento;
    }

    public static Orcamento UpdateOrcamentoFromCommand(UpdateOrcamentoCommand command, Orcamento orcamento)
    {
        orcamento.ObraId = command.ObraId;
        orcamento.Data = command.Data;
        orcamento.ObjetoProposta = command.ObjetoProposta;
        orcamento.Valor = command.Valor;
        orcamento.CondPagamento = command.CondPagamento;
        orcamento.Prazos = command.Prazos;
        orcamento.Comentarios = command.Comentarios;
        orcamento.NotasGerais = command.NotasGerais;
        orcamento.InicioObra = command.InicioObra;
        orcamento.TerminoObra = command.TerminoObra;
        orcamento.Situacao = command.Situacao;

        return orcamento;
    }

    public static async Task<int> ClearDependencies(DeleteOrcamentoCommand command, 
                                                IOrcamentoItemRepository _orcamentoItemRepository,
                                                IFluxoRepository _fluxoRepository)
    {
        var affectedItem = await _orcamentoItemRepository.DeleteByOrcamentoId(command.OrcamentoId);
        var affectedFluxo = await _fluxoRepository.DeleteByOrcamentoId(command.OrcamentoId);

        return affectedItem + affectedFluxo;
    }
}
