namespace Vezza.Domain.Entities;

public class Orcamento
{
    public Orcamento()
    {

    }

    public Orcamento(int obraId, DateTime data, string objetoProposta, decimal valor,
                     string condPagamento, string prazos, string comentarios,
                     string notasGerais, DateTime inicioObra, DateTime terminoObra,
                     string situacao)
    {
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

    public int OrcamentoId { get; set; }
    public int ObraId { get; set; }
    public DateTime? Data { get; set; }
    public string ObjetoProposta { get; set; }
    public decimal? Valor { get; set; }
    public string CondPagamento { get; set; }
    public string Prazos { get; set; }
    public string Comentarios { get; set; }
    public string NotasGerais { get; set; }
    public DateTime? InicioObra { get; set; }
    public DateTime? TerminoObra { get; set; }
    public string Situacao { get; set; }
}
