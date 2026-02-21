using BudgetBoard.WebAPI.Resources;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;
using MimeKit;

namespace BudgetBoard.Utils;

public class EmailSender(
    IConfiguration configuration,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var sender = configuration.GetValue<string>("EMAIL_SENDER");
        if (string.IsNullOrEmpty(sender))
        {
            throw new InvalidOperationException(responseLocalizer["EmailSenderConfigMissing"]);
        }

        // Some SMTP servers use a username separate from the sender email. If not set, use the sender email as username.
        var senderUsername = configuration.GetValue<string>("EMAIL_SENDER_USERNAME");
        if (string.IsNullOrEmpty(senderUsername))
        {
            senderUsername = sender;
        }

        var senderPassword = configuration.GetValue<string>("EMAIL_SENDER_PASSWORD");

        var smtpHost = configuration.GetValue<string>("EMAIL_SMTP_HOST");
        if (string.IsNullOrEmpty(smtpHost))
        {
            throw new InvalidOperationException(responseLocalizer["SmtpHostConfigMissing"]);
        }

        var smtpPort = configuration.GetValue<int?>("EMAIL_SMTP_PORT") ?? 587;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(sender));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.Auto);

        if (!string.IsNullOrEmpty(senderPassword))
        {
            await smtp.AuthenticateAsync(senderUsername, senderPassword);
        }

        await smtp.SendAsync(message);
    }
}
