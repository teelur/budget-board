using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BudgetBoard.Utils;

public class EmailSender : IEmailSender
{
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

        using (MailMessage mm = new MailMessage(sender, email))
        {
            mm.Subject = subject;
            string body = htmlMessage;
            mm.Body = body;
            mm.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = smtpHost;
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential(sender, senderPassword);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            await smtp.SendMailAsync(mm);
        }
    }
}
