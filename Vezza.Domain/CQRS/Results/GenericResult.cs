using Vezza.Domain.CQRS.Results.Interfaces;

namespace Vezza.Domain.CQRS.Results;

public class GenericResult : IGenericResult
{
    public GenericResult(int statusCode, object data, List<string> errors)
    {
        StatusCode = statusCode;
        Data = data;
        Errors = errors;
    }

    public GenericResult(int statusCode, object data)
    {
        StatusCode = statusCode;
        Data = data;
    }

    public GenericResult(int statusCode, object data, string url)
    {
        StatusCode = statusCode;
        Data = data;
        Url = url;
    }

    public GenericResult(int statusCode, List<string> errors)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public GenericResult(int statusCode, string error)
    {
        StatusCode = statusCode;
        Errors.Add(error);
    }

    public int StatusCode { get; private set; }
    public object Data { get; private set; }
    public string Url { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static GenericResult BadRequest()
    {
        return new GenericResult(400, "Comando inválido");
    }

    public static GenericResult Unauthorized()
    {
        return new GenericResult(401, "Não autorizado");
    }

    public static GenericResult NotFound()
    {
        return new GenericResult(404, "Recurso não encontrado");
    }

    public static GenericResult InternalServerError()
    {
        return new GenericResult(500, "Falha interna do servidor");
    }
}