using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;

public class AlterarSenhaCommand : Validable, ICommand
{
    public AlterarSenhaCommand(string email, string senha, string codigoEmail)
    {
        Required()
            .IsNotNullOrEmpty(email, "Email é obrigatório")
            .IsNotNullOrEmpty(senha, "Senha é obrigatória")
            .IsNotNullOrEmpty(codigoEmail, "Código enviado por email é obrigatório");

        Email = email;
        Senha = senha;
        CodigoEmail = codigoEmail;
    }

    public string Email { get; private set; }
    public string Senha { get; private set; }
    public string CodigoEmail { get; private set; }
}
