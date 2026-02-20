using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BudgetBoard.Utils;

public class EmailSender(IConfiguration configuration) : IEmailSender
{
    public IConfiguration Configuration { get; } = configuration;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var sender = Configuration.GetValue<string>("EMAIL_SENDER");
        if (string.IsNullOrEmpty(sender))
        {
            throw new ArgumentNullException(nameof(sender));
        }

        // Some SMTP servers use a username separate from the sender email. If not set, use the sender email as username.
        var senderUsername = Configuration.GetValue<string>("EMAIL_SENDER_USERNAME");
        if (string.IsNullOrEmpty(senderUsername))
        {
            senderUsername = sender;
        }

        var senderPassword = Configuration.GetValue<string>("EMAIL_SENDER_PASSWORD");

        var smtpHost = Configuration.GetValue<string>("EMAIL_SMTP_HOST");
        if (string.IsNullOrEmpty(smtpHost))
        {
            throw new ArgumentNullException(nameof(smtpHost));
        }

        var smtpPort = Configuration.GetValue<int?>("EMAIL_SMTP_PORT") ?? 587;

        using MailMessage mm = new(sender, email);
        mm.Subject = subject;
        mm.Body = htmlMessage;
        mm.IsBodyHtml = true;

        using SmtpClient smtp = new()
        {
            Host = smtpHost,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Port = smtpPort,
        };

        if (!string.IsNullOrEmpty(senderPassword))
        {
            smtp.Credentials = new NetworkCredential(senderUsername, senderPassword);
        }

        await smtp.SendMailAsync(mm);
    }
}
