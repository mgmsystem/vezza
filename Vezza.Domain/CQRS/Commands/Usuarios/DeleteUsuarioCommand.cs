using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Usuarios;
public class DeleteUsuarioCommand : Validable, ICommand
{
    public DeleteUsuarioCommand(int usuarioId)
    {
        Required()
            .IsRequired(usuarioId, "Id do usuário é obrigatório");

        UsuarioId = usuarioId;
    }

    public int UsuarioId { get; set; }
}