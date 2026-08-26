using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Legend2Toolbox.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(EmailSettings emailSettings, ILogger<SmtpEmailSender> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        email.Body = new TextPart(TextFormat.Html) { Text = body };
        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}