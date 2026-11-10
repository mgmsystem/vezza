namespace Vezza.Domain.Entities;

public class Autorizacao
{
    public Autorizacao()
    {
    }

    public Autorizacao(string email, string tipo, string codigo, DateTime validade, bool utilizado)
    {
        Email = email;
        Tipo = tipo;
        Codigo = codigo;
        Validade = validade;
        Utilizado = utilizado;
    }

    public int AutorizacaoId { get; set; }
    public string Email { get; set; }
    public string Tipo { get; set; }
    public string Codigo { get; set; }
    public DateTime Validade { get; set; }
    public bool Utilizado { get; set; }
    public int Tentativas { get; set; }
}
