using ClearPay.Application.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Identity;

/// <summary>Development: Serilog/ILogger the reset body. Production: same queue copy, never put the token in the HTTP body.</summary>
public sealed class LogAccountMailer : IAccountMailer
{
    private readonly ILogger<LogAccountMailer> _logger;
    private readonly IHostEnvironment _environment;

    public LogAccountMailer(ILogger<LogAccountMailer> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation(
                "ClearPay account mail queued (Development). To={To} Subject={Subject} Body={Body}",
                toEmail,
                subject,
                body);
        }
        else
        {
            _logger.LogInformation(
                "ClearPay account mail queued. To={To} Subject={Subject}. Token omitted from Production logs.",
                toEmail,
                subject);
        }

        return Task.CompletedTask;
    }
}
