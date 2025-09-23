using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BudgetBoard.WebAPI.Utils;

public class EmailSender : IEmailSender
{
    private static readonly ILogger<EmailSender> _logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<EmailSender>();
    public IConfiguration Configuration { get; }

    public EmailSender(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var sender = Configuration.GetValue<string>("EMAIL_SENDER");
        if (string.IsNullOrEmpty(sender))
        {
            throw new ArgumentNullException(nameof(sender));
        }

        // Some SMTP services have a separate username from the sender email address
        var senderUserName = Configuration.GetValue<string>("EMAIL_SENDER_USERNAME");
        if (string.IsNullOrEmpty(senderUserName))
        {
            _logger.LogInformation("EMAIL_SENDER_USERNAME not set, using EMAIL_SENDER as username");
            senderUserName = sender;
        }

        var senderPassword = Configuration.GetValue<string>("EMAIL_SENDER_PASSWORD");
        if (string.IsNullOrEmpty(senderPassword))
        {
            throw new ArgumentNullException(nameof(senderPassword));
        }

        var smtpHost = Configuration.GetValue<string>("EMAIL_SMTP_HOST");
        if (string.IsNullOrEmpty(smtpHost))
        {
            throw new ArgumentNullException(nameof(smtpHost));
        }

        using var mm = new MailMessage(sender, email)
        {
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };
        var smtp = new SmtpClient
        {
            Host = smtpHost,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(senderUserName, senderPassword),
            Port = 587,
        };

        await smtp.SendMailAsync(mm);
    }
}
