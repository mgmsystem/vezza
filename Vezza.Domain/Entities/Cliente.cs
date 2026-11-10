namespace Vezza.Domain.Entities;

public class Cliente
{
    public Cliente()
    {

    }

    public Cliente(string razaoSocial, string cnpjCpf, string cep, string logradouro,
                   string numero, string complemento, string bairro, string cidade,
                   string estado, string telefone)
    {
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
        DataCadastro = DateTime.Now;
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
    public DateTime? DataCadastro { get; set; }
}
