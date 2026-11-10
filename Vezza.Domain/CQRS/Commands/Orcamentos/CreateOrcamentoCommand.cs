using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Orcamentos;
public class CreateOrcamentoCommand : Validable, ICommand
{
    public CreateOrcamentoCommand(int obraId, DateTime data, string objetoProposta, decimal valor,
                                  string condPagamento, string prazos, string comentarios,
                                  string notasGerais, DateTime inicioObra, DateTime terminoObra,
                                  string situacao)
    {
        Required()
            .IsRequired(obraId, "Id da obra é obrigatório")
            .IsRequired(data, "Data da obra é obrigatória")
            .IsNotNullOrEmpty(objetoProposta, "Objeto da proposta é obrigatório")
            .IsRequired(valor, "Valor do orçamento é obrigatório");

        ObraId = obraId;
        Data = data;
        ObjetoProposta = objetoProposta;
        Valor = valor;
        CondPagamento = condPagamento;
        Prazos = prazos;
        Comentarios = comentarios;
        NotasGerais = notasGerais;
        InicioObra = inicioObra;
        TerminoObra = terminoObra;
        Situacao = situacao;
    }

    public int ObraId { get; set; }
    public DateTime Data { get; set; }
    public string ObjetoProposta { get; set; }
    public decimal Valor { get; set; }
    public string CondPagamento { get; set; }
    public string Prazos { get; set; }
    public string Comentarios { get; set; }
    public string NotasGerais { get; set; }
    public DateTime InicioObra { get; set; }
    public DateTime TerminoObra { get; set; }
    public string Situacao { get; set; }
}
