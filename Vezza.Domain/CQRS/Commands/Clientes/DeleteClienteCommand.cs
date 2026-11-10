using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Clientes;
public class DeleteClienteCommand : Validable, ICommand
{
    public DeleteClienteCommand(int clienteId)
    {
        Required()
            .IsRequired(clienteId, "Id do cliente é obrigatório");

        ClienteId = clienteId;
    }

    public int ClienteId { get; set; }
}