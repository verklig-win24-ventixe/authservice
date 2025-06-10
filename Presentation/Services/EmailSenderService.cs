using MailKit.Net.Smtp;
using MimeKit;
using Presentation.Interfaces;

namespace Presentation.Services;

public class EmailSenderService(IConfiguration config) : IEmailSenderService
{
  private readonly IConfiguration _config = config;

  public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
  {
    var email = new MimeMessage();
    email.From.Add(MailboxAddress.Parse(_config["EmailFrom"]!));
    email.To.Add(MailboxAddress.Parse(toEmail));
    email.Subject = subject;
    email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

    using var smtp = new SmtpClient();
    await smtp.ConnectAsync(_config["EmailSmtpHost"]!, int.Parse(_config["EmailSmtpPort"]!), true);
    await smtp.AuthenticateAsync(_config["EmailSmtpUser"]!, _config["EmailSmtpPass"]!);
    await smtp.SendAsync(email);
    await smtp.DisconnectAsync(true);
  }
}