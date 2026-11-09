using ClearPay.Contracts;
using ClearPay.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearPay.Reconciliation;

public sealed class ReconciliationOptions
{
    public const string SectionName = "Reconciliation";

    /// <summary>Acquirers that send a settlement file every day (names match the web app's adapters).</summary>
    public List<string> Providers { get; set; } = [];

    public static readonly IReadOnlyList<string> DefaultProviders = ["MockBankA", "MockBankB", "MockPspC"];

    public string SettlementDirectory { get; set; } = "settlement-files";

    public string NightlyCron { get; set; } = "0 2 * * *";

    public string TimeZone { get; set; } = BusinessDay.TimeZoneId;

    /// <summary>Local demo only: lets the Hangfire dashboard answer non-local requests (Docker port mapping).</summary>
    public bool DashboardOpenToAll { get; set; }

    /// <summary>Tests: Hangfire in memory instead of SQL Server.</summary>
    public bool UseMemoryHangfire { get; set; }

    public int MaxAttempts { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 5;
}

/// <summary>
/// One reconciliation run for one business day:
/// our projection + each provider's settlement file → matcher → report + lines (one SaveChanges) →
/// optional ledger check via the T-SQL team's SP → mismatch event when anything differs.
/// Re-running a day creates a new report (the old one stays as history).
/// </summary>
public sealed class ReconciliationService
{
    private readonly ReconciliationDbContext _db;
    private readonly ISettlementFileSource _files;
    private readonly ILedgerDailyTotals _ledger;
    private readonly IEventPublisher _publisher;
    private readonly ReconciliationOptions _options;
    private readonly TimeProvider _time;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(
        ReconciliationDbContext db,
        ISettlementFileSource files,
        ILedgerDailyTotals ledger,
        IEventPublisher publisher,
        ReconciliationOptions options,
        TimeProvider time,
        ILogger<ReconciliationService> logger)
    {
        _db = db;
        _files = files;
        _ledger = ledger;
        _publisher = publisher;
        _options = options;
        _time = time;
        _logger = logger;
    }

    public async Task<ReconciliationReport> RunAsync(DateOnly day, string trigger, CancellationToken cancellationToken = default)
    {
        var started = _time.GetUtcNow();
        var transactions = await _db.PaymentTransactions.AsNoTracking()
            .Where(t => t.BusinessDate == day)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var results = new List<MatchResult>();
        var bankNet = 0m;
        var oursNet = 0m;
        foreach (var provider in _options.Providers)
        {
            var ours = transactions
                .Where(t => t.Provider == provider)
                .Select(t => new SettlementLine(provider, t.Kind, t.ReferenceId.ToString(), t.Amount, t.Currency))
                .ToList();
            var bank = await _files.GetAsync(provider, day, ours, cancellationToken).ConfigureAwait(false);
            results.AddRange(ReconciliationMatcher.Match(bank, ours));
            bankNet += Net(bank);
            oursNet += Net(ours);
        }

        // Card payments for a provider we do not reconcile are a configuration gap: show them, do not hide them.
        foreach (var t in transactions.Where(t => !_options.Providers.Contains(t.Provider)))
        {
            results.Add(new MatchResult(t.Provider, t.Kind, t.ReferenceId.ToString(), MatchStatus.OnlyInClearPay, null, t.Amount, t.Currency));
            oursNet += t.Kind == PaymentTransaction.Refund ? -t.Amount : t.Amount;
        }

        var report = new ReconciliationReport
        {
            Id = Guid.NewGuid(),
            BusinessDate = day,
            Trigger = trigger,
            StartedAt = started,
            BankNetTotal = bankNet,
            ClearPayNetTotal = oursNet,
            Lines = results.Select(r => new ReconciliationReportLine
            {
                Provider = r.Provider,
                Kind = r.Kind,
                Reference = r.Reference,
                Status = r.Status,
                BankAmount = r.BankAmount,
                ClearPayAmount = r.ClearPayAmount,
                Currency = r.Currency
            }).ToList()
        };

        await AddLedgerCheckAsync(report, day, cancellationToken).ConfigureAwait(false);

        report.MatchedCount = Count(report, MatchStatus.Matched);
        report.OnlyAtBankCount = Count(report, MatchStatus.OnlyAtBank);
        report.OnlyInClearPayCount = Count(report, MatchStatus.OnlyInClearPay);
        report.AmountMismatchCount = Count(report, MatchStatus.AmountMismatch);
        report.LedgerImbalanceCount = Count(report, MatchStatus.LedgerImbalance);
        report.CompletedAt = _time.GetUtcNow();

        _db.Reports.Add(report);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation(
            "Reconciliation {BusinessDate} ({Trigger}) report {ReportId}: {Matched} matched, {OnlyAtBank} only at bank, {OnlyInClearPay} only in ClearPay, {AmountMismatch} amount differences, ledger {LedgerCheck}",
            day, trigger, report.Id, report.MatchedCount, report.OnlyAtBankCount, report.OnlyInClearPayCount,
            report.AmountMismatchCount, report.LedgerCheck);

        await PublishPendingMismatchesAsync(cancellationToken).ConfigureAwait(false);
        return report;
    }

    /// <summary>
    /// Publishes the mismatch event for every report that has differences and no confirmed publish yet.
    /// The report row is this service's outbox: if RabbitMQ is down now, the next run sends it.
    /// </summary>
    public async Task<int> PublishPendingMismatchesAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _db.Reports
            .Where(r => r.MismatchEventPublishedAt == null
                        && r.OnlyAtBankCount + r.OnlyInClearPayCount + r.AmountMismatchCount + r.LedgerImbalanceCount > 0)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var sent = 0;
        foreach (var report in pending)
        {
            var body = ContractJson.Serialize(new ReconciliationMismatchFoundV1(
                report.Id,
                report.BusinessDate,
                report.MatchedCount,
                report.OnlyAtBankCount,
                report.OnlyInClearPayCount,
                report.AmountMismatchCount,
                report.LedgerImbalanceCount,
                report.CompletedAt));
            try
            {
                await _publisher.PublishAsync(
                    new OutgoingMessage(report.Id, EventTypes.ReconciliationMismatchFound, body, report.Id, report.CompletedAt),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (BrokerUnavailableException ex)
            {
                _logger.LogWarning("Mismatch event for report {ReportId} not sent yet: {Reason}", report.Id, ex.Message);
                break;
            }

            report.MismatchEventPublishedAt = _time.GetUtcNow();
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            sent++;
        }

        return sent;
    }

    private async Task AddLedgerCheckAsync(ReconciliationReport report, DateOnly day, CancellationToken cancellationToken)
    {
        var (start, end) = BusinessDay.Window(day);
        IReadOnlyList<LedgerKindTotal>? totals;
        try
        {
            totals = await _ledger.ReadAsync(start, end, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The card reconciliation is still valid without it; the report says the check did not run.
            _logger.LogWarning("Ledger check for {BusinessDate} failed: {Reason}", day, ex.Message);
            report.LedgerCheck = "Failed";
            return;
        }

        if (totals is null)
        {
            report.LedgerCheck = "Skipped";
            return;
        }

        var imbalanced = totals.Where(t => t.Net != 0m).ToList();
        report.LedgerCheck = imbalanced.Count == 0 ? "Balanced" : "Imbalanced";
        foreach (var t in imbalanced)
        {
            report.Lines.Add(new ReconciliationReportLine
            {
                Provider = "wallet-ledger",
                Kind = t.Kind,
                Reference = $"{t.EntryCount} entries",
                Status = MatchStatus.LedgerImbalance,
                ClearPayAmount = t.Net
            });
        }
    }

    private static int Count(ReconciliationReport report, string status) => report.Lines.Count(l => l.Status == status);

    private static decimal Net(IEnumerable<SettlementLine> lines) =>
        lines.Sum(l => l.Kind == PaymentTransaction.Refund ? -l.Amount : l.Amount);
}
