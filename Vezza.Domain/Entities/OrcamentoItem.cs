namespace Vezza.Domain.Entities;

public class OrcamentoItem
{
    public OrcamentoItem()
    {
    }

    public OrcamentoItem(int orcamentoId, string descricao, decimal valor)
    {
        OrcamentoId = orcamentoId;
        Descricao = descricao;
        Valor = valor;
    }

    public int OrcamentoItemId { get; set; }
    public int OrcamentoId { get; set; }
    public string Descricao { get; set; }
    public decimal? Valor { get; set; }
}
