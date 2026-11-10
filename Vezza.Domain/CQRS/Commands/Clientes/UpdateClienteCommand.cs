using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Clientes;
public class UpdateClienteCommand : Validable, ICommand
{
    public UpdateClienteCommand(int clienteId, string razaoSocial, string cnpjCpf, string cep,
                                string logradouro, string numero, string complemento,
                                string bairro, string cidade, string estado, string telefone)
    {
        Required()
            .IsRequired(clienteId, "Id do cliente é obrigatório")
            .IsNotNullOrEmpty(razaoSocial, "Razão Social é obrigatória")
            .IsGreaterThan(razaoSocial, 2, "Razão Social deve ter no mínimo 3 caracteres")
            .IsNotNullOrEmpty(cnpjCpf, "CNPJ/CPF é obrigatório");

        ClienteId = clienteId;
        RazaoSocial = razaoSocial;
        CnpjCpf = cnpjCpf;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Telefone = telefone;
    }

    public int ClienteId { get; set; }
    public string RazaoSocial { get; set; }
    public string CnpjCpf { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string Telefone { get; set; }
}
