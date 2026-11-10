using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;

public class RegistrarCommand : Validable, ICommand
{
    public RegistrarCommand(string nome, string email, string senha)
    {
        Required()
            .IsNotNullOrEmpty(nome, "Nome é obrigatório")
            .IsNotNullOrEmpty(email, "Email é obrigatório")
            .IsEmail(email, "Email inválido")
            .IsNotNullOrEmpty(senha, "Senha é obrigatória")
            .IsGreaterThan(senha, 2, "Senha deve ter no mínimo 3 caracteres");

        Nome = nome;
        Email = email;
        Senha = senha;
    }

    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Senha { get; private set; }
}
