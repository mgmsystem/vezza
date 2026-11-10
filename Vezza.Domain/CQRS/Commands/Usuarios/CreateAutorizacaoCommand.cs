using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;

public class CreateAutorizacaoCommand : Validable, ICommand
{
    public CreateAutorizacaoCommand(string email)
    {
        Required()
            .IsNotNullOrEmpty(email, "Email é obrigatório");

        Email = email;
    }

    public string Email { get; private set; }
}
