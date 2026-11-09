using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Domain.Payments;
using ClearPay.Infrastructure.Payments;
using ClearPay.Infrastructure.Payments.Acquirers;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Tests;

/// <summary>SqlPaymentGateway against a real (SQLite in-memory) database with the mock adapters.</summary>
public sealed class PaymentGatewayTests : IDisposable
{
    private const string Merchant = "shop-1";
    private const string Visa = "4111111111111111";
    private const string Mastercard = "5555555555554444";
    private const string Troy = "9792030000000018";

    private readonly SqliteConnection _connection;
    private readonly List<ClearPayDbContext> _contexts = [];
    private readonly ThreeDsSigner _signer = new(new ThreeDsOptions { AcsSecret = "unit-test-secret" });
    private readonly ClearPayDbContext _db;
    private readonly SqlPaymentGateway _gateway;

    public PaymentGatewayTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = NewContext();
        _db.Database.EnsureCreated();
        _gateway = NewGateway(_db);
    }

    [Fact]
    public async Task Create_starts_3ds_routes_by_bin_and_stores_no_pan()
    {
        var visa = await CreateAsync("o-visa", 10m, Visa);
        var mc = await CreateAsync("o-mc", 10m, Mastercard);
        var troy = await CreateAsync("o-troy", 10m, Troy);

        visa.Status.Should().Be("Pending3DS");
        visa.Provider.Should().Be(MockBankAAdapter.ProviderName);
        mc.Provider.Should().Be(MockBankBAdapter.ProviderName);
        troy.Provider.Should().Be(MockPspCAdapter.ProviderName);
        visa.CardBin.Should().Be("411111");
        visa.CardLast4.Should().Be("1111");
        visa.ThreeDsTransactionId.Should().NotBeNullOrEmpty();

        var row = await _db.Payments.AsNoTracking().SingleAsync(p => p.Id == visa.PaymentId);
        typeof(Payment).GetProperties().Should().NotContain(p => p.Name.Contains("Pan") || p.Name == "CardNumber");
        row.CardBin.Length.Should().Be(6);
        (await PaymentEvents()).Should().HaveCount(3).And.OnlyContain(t => t == "clearpay.payment.pending_3ds");
    }

    [Fact]
    public async Task Same_merchant_order_is_duplicate_not_a_second_payment()
    {
        var first = await CreateAsync("o-dup", 10m, Visa);
        var again = await _gateway.CreateAsync(Command("o-dup", 99m, Visa));

        again.Kind.Should().Be(PaymentResultKind.DuplicateOrder);
        again.Payment!.PaymentId.Should().Be(first.PaymentId);
        (await _db.Payments.CountAsync()).Should().Be(1);
    }

    [Theory]
    [InlineData("411111111111111", "Card")]
    [InlineData("4111111111111112", "Card")]
    [InlineData(Visa, "returnUrl")]
    public async Task Invalid_input_is_rejected_before_anything_is_written(string card, string errorPart)
    {
        var returnUrl = errorPart == "returnUrl" ? "javascript:alert(1)" : "/ok";
        var outcome = await _gateway.CreateAsync(new CreatePaymentCommand(Merchant, "o-bad", 10m, "TRY", card, returnUrl));

        outcome.Kind.Should().Be(PaymentResultKind.InvalidRequest);
        outcome.Error.Should().Contain(errorPart);
        (await _db.Payments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Happy_path_authorize_capture_partial_then_full_refund()
    {
        var created = await CreateAsync("o-happy", 100m, Visa);

        var authorized = await _gateway.CompleteThreeDsAsync(SignedCallback(created, ThreeDsResult.Authenticated));
        authorized.Kind.Should().Be(PaymentResultKind.Ok);
        authorized.Payment!.Status.Should().Be("Authorized");
        authorized.Payment.ProviderReference.Should().StartWith("A-AUTH-");

        var captured = await _gateway.CaptureAsync(Merchant, created.PaymentId);
        captured.Payment!.Status.Should().Be("Captured");
        captured.Payment.CapturedAmount.Should().Be(100m);

        var partial = await _gateway.RefundAsync(new RefundPaymentCommand(Merchant, created.PaymentId, 30.25m, "one item"));
        partial.Kind.Should().Be(PaymentResultKind.Created);
        partial.Payment!.Status.Should().Be("PartiallyRefunded");
        partial.Payment.RefundedAmount.Should().Be(30.25m);

        var tooMuch = await _gateway.RefundAsync(new RefundPaymentCommand(Merchant, created.PaymentId, 69.76m, null));
        tooMuch.Kind.Should().Be(PaymentResultKind.RefundLimitExceeded);

        var rest = await _gateway.RefundAsync(new RefundPaymentCommand(Merchant, created.PaymentId, 69.75m, null));
        rest.Payment!.Status.Should().Be("Refunded");
        rest.Payment.RefundedAmount.Should().Be(100m);
        rest.Payment.Refunds.Should().HaveCount(2).And.OnlyContain(r => r.Status == "Succeeded");

        (await PaymentEvents()).Should().Equal(
            "clearpay.payment.pending_3ds",
            "clearpay.payment.authorized",
            "clearpay.payment.captured",
            "clearpay.payment.refunded",
            "clearpay.payment.refunded");
        (await _db.OutboxMessages.AllAsync(m => m.Status == OutboxStatus.Pending && m.CorrelationId == created.PaymentId))
            .Should().BeTrue();

        // The payloads are the shared contract the notification / reconciliation services deserialize.
        var rows = (await _db.OutboxMessages.AsNoTracking().ToListAsync()).OrderBy(m => m.OccurredAt).ToList();
        var capturedEvent = ClearPay.Contracts.ContractJson.Deserialize<ClearPay.Contracts.PaymentEventV1>(
            rows.Single(m => m.Type == ClearPay.Contracts.EventTypes.PaymentCaptured).Payload);
        capturedEvent.PaymentId.Should().Be(created.PaymentId);
        capturedEvent.CapturedAmount.Should().Be(100m);
        capturedEvent.Provider.Should().Be(MockBankAAdapter.ProviderName);
        capturedEvent.ProviderReference.Should().StartWith("A-AUTH-");
        var refundEvents = rows.Where(m => m.Type == ClearPay.Contracts.EventTypes.PaymentRefunded)
            .Select(m => ClearPay.Contracts.ContractJson.Deserialize<ClearPay.Contracts.PaymentEventV1>(m.Payload))
            .ToList();
        refundEvents.Select(e => e.Extra!.RefundAmount).Should().Equal(30.25m, 69.75m);
        refundEvents.Should().OnlyContain(e => e.Extra!.RefundId != null && e.Extra.Code == "Approved");
        rows.Should().OnlyContain(m => !m.Payload.Contains("411111"), "no BIN or PAN in events");
    }

    [Fact]
    public async Task Failed_otp_marks_payment_failed_without_calling_the_bank()
    {
        var created = await CreateAsync("o-otp", 50m, Mastercard);

        var outcome = await _gateway.CompleteThreeDsAsync(SignedCallback(created, ThreeDsResult.NotAuthenticated));

        outcome.Payment!.Status.Should().Be("Failed");
        outcome.Payment.FailureCode.Should().Be("3DS_FAILED");
        outcome.Payment.ProviderReference.Should().BeNull();
        (await PaymentEvents()).Last().Should().Be("clearpay.payment.failed");
    }

    [Theory]
    [InlineData(100.51, Visa, "InsufficientFunds")]
    [InlineData(100.05, Mastercard, "Declined")]
    [InlineData(100.91, Troy, "IssuerUnavailable")]
    [InlineData(100.00, "4111111111170000", "InvalidCard")]
    public async Task Bank_decline_after_3ds_is_failed_with_normalized_code(double amount, string card, string code)
    {
        var created = await CreateAsync("o-decline", (decimal)amount, card);

        var outcome = await _gateway.CompleteThreeDsAsync(SignedCallback(created, ThreeDsResult.Authenticated));

        outcome.Payment!.Status.Should().Be("Failed");
        outcome.Payment.FailureCode.Should().Be(code);
    }

    [Fact]
    public async Task Bad_signature_changes_nothing()
    {
        var created = await CreateAsync("o-sig", 10m, Visa);
        var forged = SignedCallback(created, ThreeDsResult.NotAuthenticated) with { Result = ThreeDsResult.Authenticated };
        var wrongTx = SignedCallback(created, ThreeDsResult.Authenticated) with { ThreeDsTransactionId = "other" };
        var otherKey = new ThreeDsSigner(new ThreeDsOptions { AcsSecret = "attacker" });
        var attacker = new ThreeDsCallback(
            created.PaymentId,
            created.ThreeDsTransactionId!,
            ThreeDsResult.Authenticated,
            otherKey.Sign(created.PaymentId, created.ThreeDsTransactionId!, ThreeDsResult.Authenticated, 10m));

        foreach (var callback in new[] { forged, wrongTx, attacker })
            (await _gateway.CompleteThreeDsAsync(callback)).Kind.Should().Be(PaymentResultKind.InvalidSignature);

        (await _gateway.GetAsync(created.PaymentId))!.Status.Should().Be("Pending3DS");
        (await PaymentEvents()).Should().Equal("clearpay.payment.pending_3ds");
    }

    [Fact]
    public async Task Duplicate_callback_is_idempotent()
    {
        var created = await CreateAsync("o-twice", 10m, Visa);
        var callback = SignedCallback(created, ThreeDsResult.Authenticated);

        var first = await _gateway.CompleteThreeDsAsync(callback);
        var second = await _gateway.CompleteThreeDsAsync(callback);
        var lateFailure = await _gateway.CompleteThreeDsAsync(SignedCallback(created, ThreeDsResult.NotAuthenticated));

        first.Kind.Should().Be(PaymentResultKind.Ok);
        second.Kind.Should().Be(PaymentResultKind.AlreadyProcessed);
        second.Payment!.Status.Should().Be("Authorized");
        lateFailure.Kind.Should().Be(PaymentResultKind.AlreadyProcessed);
        (await PaymentEvents()).Count(t => t == "clearpay.payment.authorized").Should().Be(1);
        (await PaymentEvents()).Should().NotContain("clearpay.payment.failed");
    }

    [Fact]
    public async Task Parallel_duplicate_callback_loses_on_version_and_reports_already_processed()
    {
        var created = await CreateAsync("o-race", 10m, Visa);
        var callback = SignedCallback(created, ThreeDsResult.Authenticated);

        // Second gateway on its own DbContext loads the row while it is still Pending3DS...
        var otherDb = NewContext();
        var other = NewGateway(otherDb, new CallbackAdapter(() => Task.CompletedTask));
        await otherDb.Payments.SingleAsync(p => p.Id == created.PaymentId);

        // ...the first callback commits, then the second one tries to save its stale copy.
        (await _gateway.CompleteThreeDsAsync(callback)).Kind.Should().Be(PaymentResultKind.Ok);
        var raced = await other.CompleteThreeDsAsync(callback);

        raced.Kind.Should().Be(PaymentResultKind.AlreadyProcessed);
        (await PaymentEvents()).Count(t => t == "clearpay.payment.authorized").Should().Be(1);
    }

    [Fact]
    public async Task Void_only_before_capture()
    {
        var a = await AuthorizedAsync("o-void", 20m);
        var voided = await _gateway.VoidAsync(Merchant, a.PaymentId);
        voided.Payment!.Status.Should().Be("Voided");
        (await _gateway.CaptureAsync(Merchant, a.PaymentId)).Kind.Should().Be(PaymentResultKind.InvalidState);

        var b = await AuthorizedAsync("o-void-late", 20m);
        await _gateway.CaptureAsync(Merchant, b.PaymentId);
        var late = await _gateway.VoidAsync(Merchant, b.PaymentId);
        late.Kind.Should().Be(PaymentResultKind.InvalidState);
        late.Payment!.Status.Should().Be("Captured");
    }

    [Fact]
    public async Task Other_merchant_cannot_touch_the_payment()
    {
        var a = await AuthorizedAsync("o-owner", 20m);
        (await _gateway.CaptureAsync("someone-else", a.PaymentId)).Kind.Should().Be(PaymentResultKind.NotFound);
        (await _gateway.RefundAsync(new RefundPaymentCommand("someone-else", a.PaymentId, 1m, null)))
            .Kind.Should().Be(PaymentResultKind.NotFound);
    }

    [Fact]
    public async Task Declined_refund_releases_the_reservation()
    {
        var a = await AuthorizedAsync("o-ref-decl", 100m);
        await _gateway.CaptureAsync(Merchant, a.PaymentId);

        // Mock trigger: refund amount ending in .05 is declined by every mock bank.
        var declined = await _gateway.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 10.05m, null));

        declined.Kind.Should().Be(PaymentResultKind.AcquirerDeclined);
        declined.Payment!.Status.Should().Be("Captured");
        declined.Payment.RefundedAmount.Should().Be(0m);
        declined.Payment.Refunds.Single().Status.Should().Be("Failed");
        (await _gateway.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 100m, null)))
            .Payment!.Status.Should().Be("Refunded");
    }

    [Fact]
    public async Task Reservation_blocks_a_concurrent_over_refund_while_the_bank_is_called()
    {
        var a = await AuthorizedAsync("o-ref-race", 100m);
        await _gateway.CaptureAsync(Merchant, a.PaymentId);

        PaymentOutcome? concurrent = null;
        var otherDb = NewContext();
        var other = NewGateway(otherDb, new CallbackAdapter(() => Task.CompletedTask));
        // While gateway #1 is "on the phone" with the bank for 70, gateway #2 asks for 50.
        var slowBank = new CallbackAdapter(async () =>
            concurrent = await other.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 50m, null)));
        var slowDb = NewContext();
        var slow = NewGateway(slowDb, slowBank);
        await SetProviderAsync(a.PaymentId, CallbackAdapter.ProviderName);

        var first = await slow.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 70m, null));

        concurrent!.Kind.Should().Be(PaymentResultKind.RefundLimitExceeded, "70 was already reserved");
        first.Payment!.Status.Should().Be("PartiallyRefunded");
        var row = await NewContext().Payments.Include(p => p.Refunds).SingleAsync(p => p.Id == a.PaymentId);
        row.RefundedAmount.Should().Be(70m);
        row.Refunds.Should().ContainSingle();
    }

    [Fact]
    public async Task Settlement_retries_when_another_refund_bumped_the_version()
    {
        var a = await AuthorizedAsync("o-ref-retry", 100m);
        await _gateway.CaptureAsync(Merchant, a.PaymentId);

        var otherDb = NewContext();
        var other = NewGateway(otherDb, new CallbackAdapter(() => Task.CompletedTask));
        var slowBank = new CallbackAdapter(async () =>
            await other.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 20m, null)));
        var slow = NewGateway(NewContext(), slowBank);
        await SetProviderAsync(a.PaymentId, CallbackAdapter.ProviderName);

        var first = await slow.RefundAsync(new RefundPaymentCommand(Merchant, a.PaymentId, 30m, null));

        first.Kind.Should().Be(PaymentResultKind.Created);
        var row = await NewContext().Payments.Include(p => p.Refunds).SingleAsync(p => p.Id == a.PaymentId);
        row.RefundedAmount.Should().Be(50m);
        row.Refunds.Should().HaveCount(2).And.OnlyContain(r => r.Status == PaymentRefundStatus.Succeeded);
        row.Status.Should().Be(PaymentStatus.PartiallyRefunded);
    }

    [Fact]
    public async Task Stale_copy_cannot_overwrite_newer_row()
    {
        var a = await AuthorizedAsync("o-stale", 100m);
        await _gateway.CaptureAsync(Merchant, a.PaymentId);

        var db1 = NewContext();
        var db2 = NewContext();
        var p1 = await db1.Payments.Include(p => p.Refunds).SingleAsync(p => p.Id == a.PaymentId);
        var p2 = await db2.Payments.Include(p => p.Refunds).SingleAsync(p => p.Id == a.PaymentId);
        db1.PaymentRefunds.Add(p1.ReserveRefund(60m, null, DateTimeOffset.UtcNow));
        db2.PaymentRefunds.Add(p2.ReserveRefund(60m, null, DateTimeOffset.UtcNow));

        await db1.SaveChangesAsync();
        var act = () => db2.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>("both passed the in-memory check; the version stops the second");
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
        _connection.Dispose();
    }

    private ClearPayDbContext NewContext()
    {
        var context = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        _contexts.Add(context);
        return context;
    }

    private SqlPaymentGateway NewGateway(ClearPayDbContext db, IAcquirerAdapter? extra = null)
    {
        var adapters = new List<IAcquirerAdapter> { new MockBankAAdapter(), new MockBankBAdapter(), new MockPspCAdapter() };
        if (extra is not null)
            adapters.Add(extra);
        var router = new AcquirerRouter(adapters, new PaymentRoutingOptions
        {
            DefaultProvider = MockBankAAdapter.ProviderName,
            BinRoutes = new Dictionary<string, string>
            {
                ["4"] = MockBankAAdapter.ProviderName,
                ["5"] = MockBankBAdapter.ProviderName,
                ["9792"] = MockPspCAdapter.ProviderName
            }
        });
        return new SqlPaymentGateway(db, router, _signer, new UtcClock(), NullLogger<SqlPaymentGateway>.Instance);
    }

    private static CreatePaymentCommand Command(string order, decimal amount, string card) =>
        new(Merchant, order, amount, "TRY", card, "https://shop.example/return");

    private async Task<PaymentView> CreateAsync(string order, decimal amount, string card)
    {
        var outcome = await _gateway.CreateAsync(Command(order, amount, card));
        outcome.Kind.Should().Be(PaymentResultKind.Created, outcome.Error);
        return outcome.Payment!;
    }

    private async Task<PaymentView> AuthorizedAsync(string order, decimal amount)
    {
        var created = await CreateAsync(order, amount, Visa);
        var outcome = await _gateway.CompleteThreeDsAsync(SignedCallback(created, ThreeDsResult.Authenticated));
        outcome.Payment!.Status.Should().Be("Authorized");
        return outcome.Payment;
    }

    private ThreeDsCallback SignedCallback(PaymentView payment, string result) =>
        new(
            payment.PaymentId,
            payment.ThreeDsTransactionId!,
            result,
            _signer.Sign(payment.PaymentId, payment.ThreeDsTransactionId!, result, payment.Amount));

    private async Task SetProviderAsync(Guid paymentId, string provider)
    {
        await using var db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Payment SET Provider = {provider} WHERE Id = {paymentId}");
    }

    private async Task<List<string>> PaymentEvents()
    {
        await using var db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        var rows = await db.OutboxMessages.AsNoTracking().ToListAsync();
        return rows.OrderBy(m => m.OccurredAt)
            .Select(m => m.Type)
            .ToList();
    }

    private sealed class UtcClock : IClock
    {
        private DateTimeOffset _now = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

        // Strictly increasing so outbox order is deterministic.
        public DateTimeOffset UtcNow => _now = _now.AddSeconds(1);
    }

    /// <summary>Approves refunds, but runs a side effect "during the bank call" to simulate a race.</summary>
    private sealed class CallbackAdapter : IAcquirerAdapter
    {
        public const string ProviderName = "SlowBank";

        private readonly Func<Task> _duringRefund;
        private bool _fired;

        public CallbackAdapter(Func<Task> duringRefund)
        {
            _duringRefund = duringRefund;
        }

        public string Name => ProviderName;

        public Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(AcquirerResult.Approved("ok", "S-AUTH"));

        public Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(AcquirerResult.Approved("ok", "S-CAP"));

        public Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(AcquirerResult.Approved("ok", "S-VOID"));

        public async Task<AcquirerResult> RefundAsync(
            AcquirerRequest request,
            decimal refundAmount,
            CancellationToken cancellationToken = default)
        {
            if (!_fired)
            {
                _fired = true;
                await _duringRefund();
            }

            return AcquirerResult.Approved("ok", "S-REF");
        }
    }
}
