using App.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace App.Modules.Identity.Infrastructure;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        _logger.LogInformation("Sending email to {To}: {Subject}\n{Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}


