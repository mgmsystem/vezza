using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;

public class UpdateUsuarioCommand : Validable, ICommand
{
    public UpdateUsuarioCommand(int usuarioId, string nome)
    {
        Required()
            .IsRequired(usuarioId, "Id do usuário é obrigatório")
            .IsNotNullOrEmpty(nome, "Nome é obrigatório");

        UsuarioId = usuarioId;
        Nome = nome;
    }

    public int UsuarioId { get; private set; }
    public string Nome { get; private set; }
}
