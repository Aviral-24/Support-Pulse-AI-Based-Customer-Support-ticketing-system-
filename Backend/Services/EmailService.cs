using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Backend.Interfaces;

namespace Backend.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer = GetRequiredSetting("Smtp:Server");
        var smtpUser = GetRequiredSetting("Smtp:User");
        var smtpPassword = GetRequiredSetting("Smtp:Pass");
        if (!int.TryParse(GetRequiredSetting("Smtp:Port"), out var smtpPort) || smtpPort is < 1 or > 65535)
        {
            throw new InvalidOperationException("Smtp:Port must be a valid TCP port.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Support Pulse AI", smtpUser));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed.");
            throw;
        }
        finally
        {
            if (smtp.IsConnected)
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }

    private string GetRequiredSetting(string key)
    {
        return _config[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Required configuration setting '{key}' is missing.");
    }
}