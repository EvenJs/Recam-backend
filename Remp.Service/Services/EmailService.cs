using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class EmailService : IEmailService
{
  private readonly IConfiguration _configuration;

  public EmailService(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public async Task SendEmailAsync(string toEmail, string subject, string body)
  {
    var host = _configuration["Email:Host"]
            ?? throw new InvalidOperationException("Email host is not configured.");
    var port = int.Parse(_configuration["Email:Port"] ?? "587");
    var username = _configuration["Email:Username"]
        ?? throw new InvalidOperationException("Email username is not configured.");
    var password = _configuration["Email:Password"]
        ?? throw new InvalidOperationException("Email password is not configured.");

    var message = new MimeMessage();
    message.From.Add(MailboxAddress.Parse(username));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = subject;
    message.Body = new TextPart("html") { Text = body };

    using var client = new SmtpClient();
    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(username, password);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
  }
}
