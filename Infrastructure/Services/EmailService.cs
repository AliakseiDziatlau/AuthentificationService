using System.Net;
using System.Net.Mail;

namespace AuthentificationService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpConfig = _configuration.GetSection("Smtp");

        var smtpClient = new SmtpClient
        {
            Host = smtpConfig["Host"],
            Port = int.Parse(smtpConfig["Port"]),
            EnableSsl = bool.Parse(smtpConfig["EnableSsl"]),
            Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpConfig["From"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }
}