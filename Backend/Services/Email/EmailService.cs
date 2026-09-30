using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Backend.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Support Pulse AI", _config["Smtp:User"]));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(_config["Smtp:Server"], int.Parse(_config["Smtp:Port"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Smtp:User"], _config["Smtp:Pass"]);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}