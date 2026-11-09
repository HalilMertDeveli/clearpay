using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using ClearPay.Application.Banking;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Messaging;
using ClearPay.Infrastructure.Observability;
using ClearPay.Web.Observability;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog.Parsing;

namespace ClearPay.Tests;

public sealed class ObservabilityTests : IClassFixture<ClearPayWebFactory>, IDisposable
{
    private readonly ClearPayWebFactory _factory;
    private readonly ConcurrentBag<Activity> _stopped = new();
    private readonly ActivityListener _listener;

    public ObservabilityTests(ClearPayWebFactory factory)
    {
        _factory = factory;
        _listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name is ClearPayTelemetry.PaymentsSourceName
                or ClearPayTelemetry.MessagingSourceName or "test.http",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _stopped.Add(a)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();

    [Fact]
    public void Trace_context_survives_rabbit_headers_as_bytes()
    {
        using var httpSource = new ActivitySource("test.http");
        using var request = httpSource.StartActivity("POST /api/transfers", ActivityKind.Server);
        request.Should().NotBeNull();

        var headers = new Dictionary<string, object>();
        using (var publish = MessageTraceContext.StartPublish("clearpay.outbox", "transfer.completed", headers))
        {
            publish.Should().NotBeNull();
            publish!.ParentSpanId.Should().Be(request!.SpanId);
            headers.Should().ContainKey("traceparent");
            headers["traceparent"].Should().Be($"00-{publish.TraceId}-{publish.SpanId}-01");
        }

        // RabbitMQ.Client hands string headers back as UTF-8 byte arrays on the consumer side.
        var delivered = headers.ToDictionary(h => h.Key, h => (object)Encoding.UTF8.GetBytes((string)h.Value));

        var parent = MessageTraceContext.Extract(delivered);
        parent.TraceId.Should().Be(request!.TraceId);

        request.Stop();
        Activity.Current = null; // consumer runs on another thread with no ambient activity
        using var consume = MessageTraceContext.StartConsume("clearpay.outbox", "transfer.completed", delivered);
        consume.Should().NotBeNull();
        consume!.TraceId.Should().Be(request.TraceId);
        consume.ParentSpanId.Should().Be(parent.SpanId);
        consume.Kind.Should().Be(ActivityKind.Consumer);
    }

    [Fact]
    public void Extract_ignores_missing_or_garbage_headers()
    {
        MessageTraceContext.Extract(null).Should().Be(default(ActivityContext));
        MessageTraceContext.Extract(new Dictionary<string, object>()).Should().Be(default(ActivityContext));
        MessageTraceContext.Extract(new Dictionary<string, object> { ["traceparent"] = "not-a-trace" })
            .Should().Be(default(ActivityContext));
        MessageTraceContext.Extract(new Dictionary<string, object> { ["traceparent"] = 42 })
            .Should().Be(default(ActivityContext));
    }

    [Theory]
    [InlineData("card 4111 1111 1111 1111 declined", "card ****1111 declined")]
    [InlineData("pan=5555555555554444", "pan=****4444")]
    [InlineData("tc 10000000146 ok", "tc 10000000146 ok")] // 11 digits: not a PAN
    [InlineData("order 1234567890123456", "order 1234567890123456")] // fails Luhn: left alone
    [InlineData("no digits", "no digits")]
    public void Pan_like_numbers_are_masked_to_last_four(string input, string expected)
    {
        SensitiveDataEnricher.MaskPans(input).Should().Be(expected);
    }

    [Fact]
    public void Sensitive_property_names_and_nested_values_are_redacted()
    {
        var evt = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Login {Email} {Password} {Request}"),
            new[]
            {
                new LogEventProperty("Password", new ScalarValue("hunter2")),
                new LogEventProperty("UserId", new ScalarValue("u-1")),
                new LogEventProperty("Request", new StructureValue(new[]
                {
                    new LogEventProperty("CardNumber", new ScalarValue("4111111111111111")),
                    new LogEventProperty("Note", new ScalarValue("paid with 4111111111111111"))
                }))
            });

        new SensitiveDataEnricher().Enrich(evt, null!);

        ((ScalarValue)evt.Properties["Password"]).Value.Should().Be(SensitiveDataEnricher.Redacted);
        ((ScalarValue)evt.Properties["UserId"]).Value.Should().Be("u-1");
        var request = (StructureValue)evt.Properties["Request"];
        ((ScalarValue)request.Properties.Single(p => p.Name == "CardNumber").Value).Value.Should().Be("***");
        ((ScalarValue)request.Properties.Single(p => p.Name == "Note").Value).Value.Should().Be("paid with ****1111");
    }

    [Theory]
    [InlineData("abc-123_X.y", true)]
    [InlineData("", false)]
    [InlineData("bad value with spaces", false)]
    [InlineData("line\nbreak", false)]
    public void Correlation_header_is_accepted_only_when_safe(string value, bool expected)
    {
        CorrelationIdMiddleware.IsSafe(value).Should().Be(expected);
    }

    [Fact]
    public async Task Every_response_carries_correlation_and_trace_ids()
    {
        var client = _factory.CreateClient();

        var plain = await client.GetAsync("/api/health");
        plain.Headers.TryGetValues(CorrelationIdMiddleware.TraceHeaderName, out var trace).Should().BeTrue();
        trace!.Single().Should().MatchRegex("^[0-9a-f]{32}$");
        plain.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single().Should().Be(trace!.Single());

        using var echo = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        echo.Headers.Add(CorrelationIdMiddleware.HeaderName, "support-ticket-42");
        var echoed = await client.SendAsync(echo);
        echoed.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single().Should().Be("support-ticket-42");

        using var hostile = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        hostile.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, "x\" injected=1 <script>");
        var replaced = await client.SendAsync(hostile);
        replaced.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single().Should().MatchRegex("^[0-9a-f]{32}$");
    }

    [Fact]
    public void Payment_ports_are_decorated_with_tracing()
    {
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ITransferExecutor>().Should().BeOfType<TracedTransferExecutor>();
        scope.ServiceProvider.GetRequiredService<IFundingExecutor>().Should().BeOfType<TracedFundingExecutor>();
    }

    [Fact]
    public async Task Transfer_decorator_emits_span_and_metrics_without_pii()
    {
        var actor = "user-" + Guid.NewGuid().ToString("N");
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

        var correlation = Guid.NewGuid();
        var sut = new TracedTransferExecutor(new StubTransfer(TransferOutcome.Created(Guid.NewGuid(), correlation)));
        await sut.ExecuteAsync(new TransferCommand(actor, "friend@example.com", 10m, "rent 4111111111111111", "key-1"));

        var span = _stopped.Single(a => (string?)a.GetTagItem("enduser.id") == actor);
        span.Source.Name.Should().Be(ClearPayTelemetry.PaymentsSourceName);
        span.GetTagItem("payment.outcome").Should().Be("Created");
        span.GetTagItem("clearpay.correlation_id").Should().Be(correlation.ToString());
        span.TagObjects.Select(t => t.Value?.ToString()).Should()
            .NotContain(v => v != null && (v.Contains("friend@example.com") || v.Contains("4111")));

        measurements.Should().Contain(m => m.Instrument == "clearpay.payments"
            && m.Tags.Any(t => t.Key == "payment.outcome" && (string?)t.Value == "Created"));
        measurements.Should().Contain(m => m.Instrument == "clearpay.payments.duration");
    }

    [Fact]
    public async Task Funding_timeout_marks_span_as_error()
    {
        var actor = "user-" + Guid.NewGuid().ToString("N");
        var sut = new TracedFundingExecutor(new StubFunding(FundingOutcome.Fail(FundingResultKind.TimedOut)));

        await sut.ExecuteAsync(new FundingCommand(actor, BankOperation.TopUp, 50m, "TR00", "key-2"));

        var span = _stopped.Single(a => (string?)a.GetTagItem("enduser.id") == actor);
        span.DisplayName.Should().Be("payment topup");
        span.Status.Should().Be(ActivityStatusCode.Error);
    }

    private sealed class StubTransfer(TransferOutcome outcome) : ITransferExecutor
    {
        public Task<TransferOutcome> ExecuteAsync(TransferCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(outcome);
    }

    private sealed class StubFunding(FundingOutcome outcome) : IFundingExecutor
    {
        public Task<FundingOutcome> ExecuteAsync(FundingCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(outcome);
    }
}
