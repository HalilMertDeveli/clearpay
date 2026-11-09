using ClearPay.Contracts;
using ClearPay.Messaging;
using ClearPay.Reconciliation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Services.Tests;

public sealed class ReconciliationMatcherTests
{
    private static SettlementLine L(string reference, decimal amount, string kind = "SALE", string provider = "MockBankA") =>
        new(provider, kind, reference, amount, "TRY");

    [Fact]
    public void Every_outcome_is_classified()
    {
        var bank = new[] { L("a", 10m), L("b", 21m), L("x", 5m), L("r1", 3m, "REFUND") };
        var ours = new[] { L("a", 10m), L("b", 20m), L("c", 7m), L("r1", 3m, "REFUND") };

        var results = ReconciliationMatcher.Match(bank, ours);

        results.Should().HaveCount(5);
        results.Single(r => r.Reference == "a").Status.Should().Be(MatchStatus.Matched);
        results.Single(r => r.Reference == "r1").Status.Should().Be(MatchStatus.Matched);
        var diff = results.Single(r => r.Reference == "b");
        diff.Status.Should().Be(MatchStatus.AmountMismatch);
        (diff.BankAmount, diff.ClearPayAmount).Should().Be((21m, 20m));
        results.Single(r => r.Reference == "x").Status.Should().Be(MatchStatus.OnlyAtBank);
        results.Single(r => r.Reference == "c").Status.Should().Be(MatchStatus.OnlyInClearPay);
    }

    [Fact]
    public void Sale_and_refund_with_the_same_reference_are_different_keys()
    {
        var results = ReconciliationMatcher.Match([L("p", 10m, "SALE")], [L("p", 10m, "REFUND")]);
        results.Select(r => r.Status).Should().BeEquivalentTo([MatchStatus.OnlyAtBank, MatchStatus.OnlyInClearPay]);
    }

    [Fact]
    public void Split_bank_lines_for_one_reference_are_summed_and_reference_case_is_ignored()
    {
        var results = ReconciliationMatcher.Match([L("ABC", 4m), L("abc", 6m)], [L("abc", 10m)]);
        results.Should().ContainSingle().Which.Status.Should().Be(MatchStatus.Matched);
    }
}

public sealed class SettlementFileTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "clearpay-settlement-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void Csv_round_trips_with_invariant_decimals()
    {
        var lines = new[] { new SettlementLine("MockBankB", "SALE", "p1", 1234.50m, "TRY"), new SettlementLine("MockBankB", "REFUND", "r1", 0.05m, "TRY") };
        var csv = SettlementCsv.Write("MockBankB", new DateOnly(2026, 11, 9), lines);

        csv.Should().Contain("SALE,p1,1234.50,TRY").And.Contain("REFUND,r1,0.05,TRY").And.StartWith("# provider=MockBankB;date=2026-11-09");
        SettlementCsv.Parse("MockBankB", csv).Should().Equal(lines);
    }

    [Theory]
    [InlineData("SALE,p1,12.00", "expected 4 fields")]
    [InlineData("CHARGEBACK,p1,12.00,TRY", "unknown record type")]
    [InlineData("SALE,p1,12;00,TRY", "not a positive number")]
    [InlineData("SALE,p1,-5.00,TRY", "not a positive number")]
    [InlineData("SALE,,5.00,TRY", "empty reference")]
    public void Broken_line_fails_with_its_line_number(string line, string reason)
    {
        var act = () => SettlementCsv.Parse("MockBankA", SettlementCsv.Header + "\n" + line + "\n");
        act.Should().Throw<FormatException>().WithMessage($"MockBankA line 2: *{reason}*");
    }

    [Fact]
    public void Mock_bank_applies_the_documented_kurus_rules()
    {
        var ours = new[]
        {
            new SettlementLine("MockBankA", "SALE", "ok", 100.00m, "TRY"),
            new SettlementLine("MockBankA", "SALE", "diff", 10.37m, "TRY"),
            new SettlementLine("MockBankA", "SALE", "missing", 10.73m, "TRY"),
            new SettlementLine("MockBankA", "SALE", "extra-0001", 10.66m, "TRY"),
            new SettlementLine("MockBankA", "REFUND", "refund", 10.37m, "TRY")
        };

        var bank = MockSettlementGenerator.BankView(ours);

        bank.Should().Contain(l => l.Reference == "ok" && l.Amount == 100.00m);
        bank.Should().Contain(l => l.Reference == "diff" && l.Amount == 11.37m);
        bank.Should().NotContain(l => l.Reference == "missing");
        bank.Should().Contain(l => l.Reference == "extra-0001").And.Contain(l => l.Reference == "BANK-EXTRA-00");
        bank.Should().Contain(l => l.Reference == "refund" && l.Amount == 10.37m, "the rules only touch SALE lines");
    }

    [Fact]
    public async Task Received_file_is_used_as_is_and_generated_file_is_rewritten()
    {
        var source = new FolderSettlementFileSource(_folder);
        var day = new DateOnly(2026, 11, 9);
        var ours = new[] { new SettlementLine("MockPspC", "SALE", "p1", 10m, "TRY") };

        (await source.GetAsync("MockPspC", day, ours, CancellationToken.None)).Should().ContainSingle();
        File.ReadAllText(source.PathFor("MockPspC", day)).Should().Contain(SettlementCsv.GeneratedMarker);

        // A file "from the bank" (no generator marker) wins over the generator.
        await File.WriteAllTextAsync(source.PathFor("MockPspC", day), SettlementCsv.Header + "\nSALE,zz,1.00,TRY\n");
        var received = await source.GetAsync("MockPspC", day, ours, CancellationToken.None);
        received.Should().ContainSingle().Which.Reference.Should().Be("zz");
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }
}

public sealed class BusinessDayTests
{
    [Fact]
    public void Istanbul_day_is_used_not_the_utc_day()
    {
        // 22:30 UTC on 8 Nov = 01:30 on 9 Nov in Istanbul (UTC+3).
        BusinessDay.Of(new DateTimeOffset(2026, 11, 8, 22, 30, 0, TimeSpan.Zero)).Should().Be(new DateOnly(2026, 11, 9));
        BusinessDay.Of(new DateTimeOffset(2026, 11, 8, 20, 59, 0, TimeSpan.Zero)).Should().Be(new DateOnly(2026, 11, 8));
    }

    [Fact]
    public void Window_and_previous_day()
    {
        var (start, end) = BusinessDay.Window(new DateOnly(2026, 11, 9));
        start.Should().Be(new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.FromHours(3)));
        end.Should().Be(new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.FromHours(3)));

        // The 02:00 Istanbul run on 10 Nov reconciles 9 Nov.
        BusinessDay.Previous(new DateTimeOffset(2026, 11, 9, 23, 0, 0, TimeSpan.Zero)).Should().Be(new DateOnly(2026, 11, 9));
    }
}

public sealed class PaymentProjectionHandlerTests : IDisposable
{
    private readonly SqliteDb _sqlite = new();

    [Fact]
    public async Task Capture_and_refund_become_sale_and_refund_rows_once()
    {
        await using var db = _sqlite.Reconciliation();
        var paymentId = Guid.NewGuid();
        var refundId = Guid.NewGuid();
        var at = new DateTimeOffset(2026, 11, 8, 22, 0, 0, TimeSpan.Zero); // 01:00 on 9 Nov in Istanbul
        var capture = Events.Payment(EventTypes.PaymentCaptured, 120.00m, paymentId: paymentId, occurredAt: at, provider: "MockBankB");
        var refund = Events.Payment(EventTypes.PaymentRefunded, 120.00m, paymentId: paymentId, occurredAt: at, provider: "MockBankB",
            extra: new PaymentEventExtra(RefundId: refundId, RefundAmount: 20.10m, Code: "Approved"));

        var handler = new PaymentProjectionHandler(db);
        await handler.HandleAsync(capture, CancellationToken.None);
        await handler.HandleAsync(refund, CancellationToken.None);
        await handler.HandleAsync(capture with { Attempt = 2 }, CancellationToken.None);

        var rows = await db.PaymentTransactions.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(2);
        rows.Single(r => r.Kind == "SALE").Should().BeEquivalentTo(new { ReferenceId = paymentId, Amount = 120.00m, Provider = "MockBankB", BusinessDate = new DateOnly(2026, 11, 9) });
        rows.Single(r => r.Kind == "REFUND").Should().BeEquivalentTo(new { ReferenceId = refundId, Amount = 20.10m, PaymentId = paymentId });
    }

    [Fact]
    public void Refund_without_refund_details_is_poison()
    {
        var act = () => PaymentProjectionHandler.ToTransaction(Events.Payment(EventTypes.PaymentRefunded));
        act.Should().Throw<PoisonMessageException>();
    }

    public void Dispose() => _sqlite.Dispose();
}

public sealed class ReconciliationServiceTests : IDisposable
{
    private static readonly DateOnly Day = new(2026, 11, 9);
    private readonly SqliteDb _sqlite = new();
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "clearpay-recon-" + Guid.NewGuid().ToString("N"));
    private readonly RecordingEventPublisher _publisher = new();
    private readonly FakeLedger _ledger = new();

    [Fact]
    public async Task Nightly_run_finds_every_kind_of_difference_and_publishes_one_event()
    {
        await using var db = _sqlite.Reconciliation();
        await SeedAsync(db, ("MockBankA", "SALE", 100.00m), ("MockBankA", "SALE", 10.37m), ("MockBankB", "SALE", 10.73m),
            ("MockPspC", "SALE", 10.66m), ("MockBankB", "REFUND", 5.00m));
        // Another day must not leak into this report.
        db.PaymentTransactions.Add(Tx("MockBankA", "SALE", 999m, Day.AddDays(-1)));
        await db.SaveChangesAsync();
        _ledger.Totals = [new LedgerKindTotal("Transfer", 4, 50m, 50m, 0m), new LedgerKindTotal("TopUp", 3, 30m, 20m, 10m)];

        var report = await Service(db).RunAsync(Day, "nightly");

        report.MatchedCount.Should().Be(3, "100.00, the 10.66 sale itself and the refund");
        report.AmountMismatchCount.Should().Be(1);
        report.OnlyInClearPayCount.Should().Be(1);
        report.OnlyAtBankCount.Should().Be(1);
        report.LedgerImbalanceCount.Should().Be(1);
        report.LedgerCheck.Should().Be("Imbalanced");
        report.ClearPayNetTotal.Should().Be(100.00m + 10.37m + 10.73m + 10.66m - 5.00m);
        report.BankNetTotal.Should().Be(100.00m + 11.37m + 10.66m + 10.66m - 5.00m);
        report.Lines.Should().Contain(l => l.Status == MatchStatus.LedgerImbalance && l.Kind == "TopUp" && l.ClearPayAmount == 10m);

        var saved = await db.Reports.AsNoTracking().Include(r => r.Lines).SingleAsync();
        saved.Lines.Should().HaveCount(7);
        saved.MismatchEventPublishedAt.Should().NotBeNull();

        var published = _publisher.Published.Should().ContainSingle().Subject;
        published.MessageId.Should().Be(report.Id, "report id = message id, so a re-send is deduplicated");
        published.Type.Should().Be(EventTypes.ReconciliationMismatchFound);
        var body = ContractJson.Deserialize<ReconciliationMismatchFoundV1>(published.Body);
        body.BusinessDate.Should().Be(Day);
        body.AmountMismatchCount.Should().Be(1);

        File.Exists(Path.Combine(_folder, "MockBankA", "2026-11-09.csv")).Should().BeTrue();
    }

    [Fact]
    public async Task Clean_day_publishes_nothing_and_ledger_check_can_be_skipped()
    {
        await using var db = _sqlite.Reconciliation();
        await SeedAsync(db, ("MockBankA", "SALE", 100.00m));
        _ledger.Totals = null;

        var report = await Service(db).RunAsync(Day, "manual");

        report.HasDifferences.Should().BeFalse();
        report.LedgerCheck.Should().Be("Skipped");
        _publisher.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task Event_not_confirmed_now_is_sent_by_the_next_run()
    {
        await using var db = _sqlite.Reconciliation();
        await SeedAsync(db, ("MockBankA", "SALE", 10.73m));
        _publisher.Down = true;

        var first = await Service(db).RunAsync(Day, "nightly");
        (await db.Reports.AsNoTracking().SingleAsync()).MismatchEventPublishedAt.Should().BeNull();

        _publisher.Down = false;
        (await Service(db).PublishPendingMismatchesAsync()).Should().Be(1);
        _publisher.Published.Single().MessageId.Should().Be(first.Id);
    }

    [Fact]
    public async Task Broken_bank_file_fails_the_run_instead_of_a_clean_report()
    {
        await using var db = _sqlite.Reconciliation();
        Directory.CreateDirectory(Path.Combine(_folder, "MockBankA"));
        await File.WriteAllTextAsync(Path.Combine(_folder, "MockBankA", "2026-11-09.csv"), "record_type,reference,amount,currency\nSALE,x,abc,TRY\n");

        var act = () => Service(db).RunAsync(Day, "nightly");

        await act.Should().ThrowAsync<FormatException>();
        (await db.Reports.CountAsync()).Should().Be(0);
    }

    private ReconciliationService Service(ReconciliationDbContext db) => new(
        db,
        new FolderSettlementFileSource(_folder),
        _ledger,
        _publisher,
        new ReconciliationOptions { Providers = ReconciliationOptions.DefaultProviders.ToList() },
        TimeProvider.System,
        NullLogger<ReconciliationService>.Instance);

    private static async Task SeedAsync(ReconciliationDbContext db, params (string Provider, string Kind, decimal Amount)[] rows)
    {
        foreach (var (provider, kind, amount) in rows)
            db.PaymentTransactions.Add(Tx(provider, kind, amount, Day));
        await db.SaveChangesAsync();
    }

    private static PaymentTransaction Tx(string provider, string kind, decimal amount, DateOnly day) => new()
    {
        Id = Guid.NewGuid(),
        Kind = kind,
        PaymentId = Guid.NewGuid(),
        ReferenceId = Guid.NewGuid(),
        Provider = provider,
        MerchantId = "demo-shop",
        Amount = amount,
        Currency = "TRY",
        OccurredAt = new DateTimeOffset(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.FromHours(3)),
        BusinessDate = day,
        SourceMessageId = Guid.NewGuid()
    };

    public void Dispose()
    {
        _sqlite.Dispose();
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    private sealed class FakeLedger : ILedgerDailyTotals
    {
        public IReadOnlyList<LedgerKindTotal>? Totals { get; set; }

        public Task<IReadOnlyList<LedgerKindTotal>?> ReadAsync(DateTimeOffset dayStart, DateTimeOffset dayEnd, CancellationToken cancellationToken) =>
            Task.FromResult(Totals);
    }
}
