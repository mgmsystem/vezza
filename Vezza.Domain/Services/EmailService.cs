using System.Net;
using System.Net.Mail;

namespace Vezza.Domain.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderEmail;
    private readonly string _senderDisplayName;
    private readonly bool _enableSsl;

    public EmailService(
        string smtpHost,
        int smtpPort,
        string smtpUsername,
        string smtpPassword,
        string senderEmail,
        string senderDisplayName,
        bool enableSsl)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
        _senderEmail = senderEmail;
        _senderDisplayName = senderDisplayName;
        _enableSsl = enableSsl;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpClient = new SmtpClient(_smtpHost)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception)
        {
            // A falha é sinalizada ao chamador sem registrar credenciais ou conteúdo da mensagem.
            Console.WriteLine("Falha ao enviar e-mail.");
            return false;
        }
    }

    public string TemplateCodigoAutorizacao(string codigo)
    {
        return @"
            <html>
                <head>
                    <title>Email de Confirmação</title>
                    <style>
                        body { 
                            text-align: center; 
                            margin: auto; 
                            max-width: 600px; /* Largura máxima para o conteúdo */
                        }
                        .codigo { 
                            font-size: 24px; /* Ajuste conforme necessário */
                            color: #333; /* Cor do texto */
                            margin: 20px 0; /* Espaçamento para separar do texto circundante */
                        }
                    </style>
                </head>
                <body>
                    <h2>Serviços Gutimobi</h2>
                    <h4>Código para prosseguir com a troca de senha</h4>
                    <div class='codigo'><b>" + codigo + @"</b></div>
                    <h4>Caso não tenha solicitado, ignore essa mensagem</h4>
                </body>
            </html>";
    }
}
