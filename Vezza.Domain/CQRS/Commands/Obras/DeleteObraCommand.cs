using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Obras;
public class DeleteObraCommand : Validable, ICommand
{
    public DeleteObraCommand(int obraId)
    {
        Required()
            .IsRequired(obraId, "Id da obra é obrigatório");

        ObraId = obraId;
    }

    public int ObraId { get; set; }
}
