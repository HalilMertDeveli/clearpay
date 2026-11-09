using System.Diagnostics;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;

namespace ClearPay.Infrastructure.Observability;

/// <summary>
/// Decorator: wraps the real <see cref="ITransferExecutor"/> in a <c>ClearPay.Payments</c> span and
/// records the payment counter/histogram. The ledger code itself stays unaware of OpenTelemetry.
/// Only ids and enums go on the span — never recipient e-mail, description or card data.
/// </summary>
public sealed class TracedTransferExecutor : ITransferExecutor
{
    private readonly ITransferExecutor _inner;

    public TracedTransferExecutor(ITransferExecutor inner) => _inner = inner;

    public async Task<TransferOutcome> ExecuteAsync(TransferCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ClearPayTelemetry.Payments.StartActivity("payment transfer");
        activity?.SetTag("payment.kind", "transfer");
        activity?.SetTag("enduser.id", command.ActorUserId);
        var started = Stopwatch.GetTimestamp();
        try
        {
            var outcome = await _inner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            var kind = outcome.Kind.ToString();
            activity?.SetTag("payment.outcome", kind);
            activity?.SetTag("clearpay.correlation_id", outcome.CorrelationId.ToString());
            if (outcome.TransferId is { } id)
                activity?.SetTag("clearpay.transfer_id", id.ToString());
            if (outcome.Kind == TransferResultKind.Unavailable)
                activity?.SetStatus(ActivityStatusCode.Error, kind);
            ClearPayTelemetry.RecordPayment("transfer", kind, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return outcome;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            ClearPayTelemetry.RecordPayment("transfer", "Exception", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>Same decorator for top-up / withdraw through the fake bank gateway.</summary>
public sealed class TracedFundingExecutor : IFundingExecutor
{
    private readonly IFundingExecutor _inner;

    public TracedFundingExecutor(IFundingExecutor inner) => _inner = inner;

    public async Task<FundingOutcome> ExecuteAsync(FundingCommand command, CancellationToken cancellationToken = default)
    {
        var paymentKind = command.Operation.ToString().ToLowerInvariant();
        using var activity = ClearPayTelemetry.Payments.StartActivity($"payment {paymentKind}");
        activity?.SetTag("payment.kind", paymentKind);
        activity?.SetTag("enduser.id", command.ActorUserId);
        var started = Stopwatch.GetTimestamp();
        try
        {
            var outcome = await _inner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            var kind = outcome.Kind.ToString();
            activity?.SetTag("payment.outcome", kind);
            activity?.SetTag("clearpay.correlation_id", outcome.CorrelationId.ToString());
            if (outcome.Kind is FundingResultKind.TimedOut or FundingResultKind.GatewayFailed)
                activity?.SetStatus(ActivityStatusCode.Error, kind);
            ClearPayTelemetry.RecordPayment(paymentKind, kind, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return outcome;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            ClearPayTelemetry.RecordPayment(paymentKind, "Exception", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}
