using Vezza.Domain.CQRS.Commands.Obras;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;

namespace Vezza.Domain.Services;

public static class ObraService
{
    public static Obra CreateObraFromCommand(CreateObraCommand command)
    {
        var obra = new Obra();
        obra.ClienteId = command.ClienteId;
        obra.Descricao = command.Descricao;
        obra.Cep = command.Cep;
        obra.Logradouro = command.Logradouro;
        obra.Numero = command.Numero;
        obra.Complemento = command.Complemento;
        obra.Bairro = command.Bairro;
        obra.Cidade = command.Cidade;
        obra.Estado = command.Estado;
        obra.DataCadastro = DateTime.Now;

        return obra;
    }

    public static Obra UpdateObraFromCommand(UpdateObraCommand command, Obra obra)
    {
        obra.ClienteId = command.ClienteId;
        obra.Descricao = command.Descricao;
        obra.Cep = command.Cep;
        obra.Logradouro = command.Logradouro;
        obra.Numero = command.Numero;
        obra.Complemento = command.Complemento;
        obra.Bairro = command.Bairro;
        obra.Cidade = command.Cidade;
        obra.Estado = command.Estado;

        return obra;
    }

    public static async Task<bool> HasDependencies(DeleteObraCommand command, IOrcamentoRepository _orcamentoRepository)
    {
        var orcamento = await _orcamentoRepository.GetFirstByObraId(command.ObraId);

        if (orcamento != null)
        {
            command.SetInvalid("Obra vinculada a orçamentos não pode ser excluída");
            return true;
        }

        return false;
    }
}
