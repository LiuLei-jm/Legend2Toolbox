namespace Legend2Toolbox.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("=============================================");
        _logger.LogInformation("[本地开发邮件模拟] 成功向外发一封邮件！");
        _logger.LogInformation("收件人: {To}", to);
        _logger.LogInformation("主  题: {Subject}", subject);
        _logger.LogInformation("内  容: {body}", body);
        _logger.LogInformation("=============================================");

        return Task.CompletedTask;
    }
}
