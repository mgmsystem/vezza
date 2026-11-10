namespace Vezza.Domain.Entities;

public class Usuario
{
    public Usuario()
    {

    }

    public Usuario(string nome, string email, string senha, int nivel)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
        Nivel = nivel;
    }

    public int UsuarioId { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public int Nivel { get; set; }
}
