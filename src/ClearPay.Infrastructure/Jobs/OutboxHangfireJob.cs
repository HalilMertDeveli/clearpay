using ClearPay.Application.Ports;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Infrastructure.Jobs;

public sealed class OutboxHangfireJob
{
    private readonly IOutboxProcessor _processor;

    public OutboxHangfireJob(IOutboxProcessor processor)
    {
        _processor = processor;
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(60)]
    public Task Run() => _processor.ProcessPendingAsync(CancellationToken.None);
}
