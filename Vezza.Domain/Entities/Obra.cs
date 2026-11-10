namespace Vezza.Domain.Entities;

public class Obra
{
    public Obra()
    {

    }

    public Obra(int clienteId, string descricao, string cep, string logradouro,
                string numero, string complemento, string bairro, string cidade,
                string estado)
    {
        ClienteId = clienteId;
        Descricao = descricao;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        DataCadastro = DateTime.Now;
    }

    public int ObraId { get; set; }
    public int ClienteId { get; set; }
    public string Descricao { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public DateTime? DataCadastro { get; set; }
}
