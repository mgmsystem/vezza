using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Shared.Validations;

namespace Vezza.Domain.CQRS.Commands.Obras;
public class CreateObraCommand : Validable, ICommand
{
    public CreateObraCommand(int clienteId, string descricao, string cep, string logradouro,
                             string numero, string complemento, string bairro, string cidade,
                             string estado)
    {
        Required()
            .IsRequired(clienteId, "Id do cliente é obrigatório")
            .IsNotNullOrEmpty(descricao, "Descrição é obrigatória")
            .IsGreaterThan(descricao, 2, "Descrição deve ter no mínimo 3 caracteres");

        ClienteId = clienteId;
        Descricao = descricao;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
    }

    public int ClienteId { get; set; }
    public string Descricao { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
}
