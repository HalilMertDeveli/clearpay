using System.Globalization;
using Hangfire;

namespace ClearPay.Reconciliation;

/// <summary>
/// Hangfire entry points. The recurring job runs at 02:00 Europe/Istanbul and reconciles the previous
/// Istanbul day. Arguments are strings because Hangfire stores them as JSON.
/// </summary>
public sealed class NightlyReconciliationJob
{
    public const string RecurringJobId = "reconciliation-nightly";

    private readonly ReconciliationService _service;
    private readonly TimeProvider _time;

    public NightlyReconciliationJob(ReconciliationService service, TimeProvider time)
    {
        _service = service;
        _time = time;
    }

    /// <summary>A failed run (e.g. unreadable bank file) is retried by Hangfire, then shows as Failed on the dashboard.</summary>
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public Task RunPreviousDayAsync() =>
        _service.RunAsync(BusinessDay.Previous(_time.GetUtcNow()), "nightly");

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public Task RunForDateAsync(string businessDate) =>
        _service.RunAsync(DateOnly.ParseExact(businessDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), "manual");
}
