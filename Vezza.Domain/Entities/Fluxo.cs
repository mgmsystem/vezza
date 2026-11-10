namespace Vezza.Domain.Entities;

public class Fluxo
{
    public int FluxoId { get; set; }
    public int? OrcamentoId { get; set; }
    public DateTime? Data { get; set; }
    public string Descricao { get; set; }
    public string DebitoCredito { get; set; }
    public decimal? Valor { get; set; }
    public string Categoria { get; set; }
}
