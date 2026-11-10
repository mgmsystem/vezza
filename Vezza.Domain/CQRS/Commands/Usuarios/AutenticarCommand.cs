using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;

public class AutenticarCommand : Validable, ICommand
{
    public AutenticarCommand(string email, string senha)
    {
        Required()
            .IsNotNullOrEmpty(email, "Email é obrigatório")
            .IsNotNullOrEmpty(senha, "Senha é obrigatória");

        Email = email;
        Senha = senha;
    }
        
    public string Email { get; private set; }
    public string Senha { get; private set; }
}
