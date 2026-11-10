using Vezza.Domain.CQRS.Commands.Clientes;
using Vezza.Domain.Entities;
using Vezza.Domain.Repositories;

namespace Vezza.Domain.Services;

public static class ClienteService
{
    public static Cliente CreateClienteFromCommand(CreateClienteCommand command)
    {
        var cliente = new Cliente();
        cliente.RazaoSocial = command.RazaoSocial;
        cliente.CnpjCpf = command.CnpjCpf;
        cliente.Cep = command.Cep;
        cliente.Logradouro = command.Logradouro;
        cliente.Numero = command.Numero;
        cliente.Complemento = command.Complemento;
        cliente.Bairro = command.Bairro;
        cliente.Cidade = command.Cidade;
        cliente.Estado = command.Estado;
        cliente.Telefone = command.Telefone;
        cliente.DataCadastro = DateTime.Now;

        return cliente;
    }

    public static Cliente UpdateClienteFromCommand(UpdateClienteCommand command, Cliente cliente)
    {
        cliente.RazaoSocial = command.RazaoSocial;
        cliente.CnpjCpf = command.CnpjCpf;
        cliente.Cep = command.Cep;
        cliente.Logradouro = command.Logradouro;
        cliente.Numero = command.Numero;
        cliente.Complemento = command.Complemento;
        cliente.Bairro = command.Bairro;
        cliente.Cidade = command.Cidade;
        cliente.Estado = command.Estado;
        cliente.Telefone = command.Telefone;

        return cliente;
    }

    public static async Task<bool> HasDependencies(DeleteClienteCommand command, IObraRepository _obraRepository)
    {
        var obra = await _obraRepository.GetFirstByClienteId(command.ClienteId);

        if (obra != null)
        {
            command.SetInvalid("Cliente vinculado a obras não pode ser excluído");
            return true;
        }

        return false;
    }
}
