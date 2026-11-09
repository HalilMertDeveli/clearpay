using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Observability;
using ClearPay.Infrastructure.Payments.Acquirers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Tests;

/// <summary>
/// The mock 3DS card flow emits <c>ClearPay.Payments</c> spans and metrics: one span per use case,
/// a child span per acquirer call, and no card data on any tag.
/// </summary>
public sealed class CardPaymentTracingTests : IClassFixture<ClearPayWebFactory>, IDisposable
{
    private readonly ClearPayWebFactory _factory;
    private readonly ConcurrentBag<Activity> _stopped = new();
    private readonly ActivityListener _listener;

    public CardPaymentTracingTests(ClearPayWebFactory factory)
    {
        _factory = factory;
        _listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == ClearPayTelemetry.PaymentsSourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _stopped.Add(a)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();

    [Fact]
    public void Gateway_and_every_acquirer_are_decorated_with_tracing()
    {
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IPaymentGateway>().Should().BeOfType<TracedPaymentGateway>();

        var adapters = scope.ServiceProvider.GetServices<IAcquirerAdapter>().ToList();
        adapters.Should().HaveCount(3).And.AllBeOfType<TracedAcquirerAdapter>();
        adapters.Select(a => a.Name).Should().BeEquivalentTo(
            MockBankAAdapter.ProviderName, MockBankBAdapter.ProviderName, MockPspCAdapter.ProviderName);
    }

    [Fact]
    public async Task Full_3ds_flow_produces_payment_and_acquirer_spans_without_card_data()
    {
        const string card = "5555555555554444";
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var (paymentId, acsUrl) = await ThreeDsFlowTests.CreatePaymentAsync(client, 120.00m, card);
        var fields = await ThreeDsFlowTests.SubmitOtpAsync(client, acsUrl, "123456");

        var forged = new Dictionary<string, string>(fields) { ["signature"] = new string('0', 64) };
        (await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(forged))).StatusCode
            .Should().Be(HttpStatusCode.BadRequest);
        (await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(fields))).StatusCode
            .Should().Be(HttpStatusCode.Redirect);
        (await ThreeDsFlowTests.Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/capture")).StatusCode
            .Should().Be(HttpStatusCode.OK);

        var spans = _stopped.Where(a => (string?)a.GetTagItem("clearpay.payment_id") == paymentId.ToString()).ToList();

        var create = spans.Single(a => a.DisplayName == "payment card_create");
        create.GetTagItem("payment.outcome").Should().Be("Created");
        create.GetTagItem("payment.status").Should().Be("Pending3DS");
        create.GetTagItem("payment.provider").Should().Be(MockBankBAdapter.ProviderName);

        var threeDs = spans.Where(a => a.DisplayName == "payment card_3ds").ToList();
        threeDs.Should().HaveCount(2);
        var rejected = threeDs.Single(a => (string?)a.GetTagItem("payment.outcome") == "InvalidSignature");
        rejected.Status.Should().Be(ActivityStatusCode.Error, "a forged 3DS result is what on-call wants to find");
        var accepted = threeDs.Single(a => (string?)a.GetTagItem("payment.outcome") == "Ok");
        accepted.GetTagItem("payment.status").Should().Be("Authorized");

        var authorize = spans.Single(a => a.DisplayName == "acquirer authorize");
        authorize.ParentSpanId.Should().Be(accepted.SpanId, "the bank call is a child of the callback span");
        authorize.GetTagItem("acquirer.name").Should().Be(MockBankBAdapter.ProviderName);
        authorize.GetTagItem("acquirer.response_code").Should().Be("Approved");

        spans.Should().Contain(a => a.DisplayName == "payment card_capture");
        spans.Should().Contain(a => a.DisplayName == "acquirer capture");

        spans.SelectMany(a => a.TagObjects).Select(t => t.Value?.ToString()).Should()
            .NotContain(v => v != null && (v.Contains(card) || v.Contains("555555")));
    }

    [Fact]
    public async Task Gateway_decorator_records_card_metrics_and_marks_conflicts_as_errors()
    {
        var merchant = "m-" + Guid.NewGuid().ToString("N");
        var measurements = new ConcurrentBag<(string Instrument, KeyValuePair<string, object?>[] Tags)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == ClearPayTelemetry.MeterName)
                    l.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<long>((i, _, tags, _) => measurements.Add((i.Name, tags.ToArray())));
        meterListener.SetMeasurementEventCallback<double>((i, _, tags, _) => measurements.Add((i.Name, tags.ToArray())));
        meterListener.Start();

        var sut = new TracedPaymentGateway(new StubGateway(
            PaymentOutcome.Fail(PaymentResultKind.ConcurrencyConflict, "changed by another request")));
        await sut.RefundAsync(new RefundPaymentCommand(merchant, Guid.NewGuid(), 10m, null));

        var span = _stopped.Single(a => (string?)a.GetTagItem("clearpay.merchant_id") == merchant);
        span.DisplayName.Should().Be("payment card_refund");
        span.Status.Should().Be(ActivityStatusCode.Error);

        measurements.Should().Contain(m => m.Instrument == "clearpay.payments"
            && m.Tags.Any(t => t.Key == "payment.kind" && (string?)t.Value == "card_refund")
            && m.Tags.Any(t => t.Key == "payment.outcome" && (string?)t.Value == "ConcurrencyConflict"));
        measurements.Should().Contain(m => m.Instrument == "clearpay.payments.duration");
    }

    [Fact]
    public async Task Issuer_unavailable_marks_the_acquirer_span_as_error()
    {
        var paymentId = Guid.NewGuid();
        var sut = new TracedAcquirerAdapter(new MockBankAAdapter());

        // Kuruş .91 is the mock "issuer unavailable" trigger (docs/PAYMENTS.md).
        var result = await sut.AuthorizeAsync(new AcquirerRequest(paymentId, 10.91m, "TRY", "411111", "1111", "3ds-1", null));

        result.Code.Should().Be(AcquirerResponseCode.IssuerUnavailable);
        var span = _stopped.Single(a => (string?)a.GetTagItem("clearpay.payment_id") == paymentId.ToString());
        span.DisplayName.Should().Be("acquirer authorize");
        span.Kind.Should().Be(ActivityKind.Client);
        span.Status.Should().Be(ActivityStatusCode.Error);
        span.GetTagItem("acquirer.raw_code").Should().Be("91");
    }

    private sealed class StubGateway(PaymentOutcome outcome) : IPaymentGateway
    {
        public Task<PaymentOutcome> CreateAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default) => Task.FromResult(outcome);

        public Task<PaymentOutcome> CompleteThreeDsAsync(ThreeDsCallback callback, CancellationToken cancellationToken = default) => Task.FromResult(outcome);

        public Task<PaymentOutcome> CaptureAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) => Task.FromResult(outcome);

        public Task<PaymentOutcome> VoidAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) => Task.FromResult(outcome);

        public Task<PaymentOutcome> RefundAsync(RefundPaymentCommand command, CancellationToken cancellationToken = default) => Task.FromResult(outcome);

        public Task<PaymentView?> GetAsync(Guid paymentId, CancellationToken cancellationToken = default) => Task.FromResult<PaymentView?>(null);
    }
}
