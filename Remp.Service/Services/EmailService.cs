using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class EmailService : IEmailService
{
  public async Task SendEmailAsync(string toEmail, string subject, string body)
  {
    // TODO: Implement with SendGrid or SMTP later
    await Task.CompletedTask;
  }
}
