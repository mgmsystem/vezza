namespace Vezza.Domain.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    string TemplateCodigoAutorizacao(string codigo);
}
