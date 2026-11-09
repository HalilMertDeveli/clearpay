namespace ClearPay.Domain.Payments;

/// <summary>
/// The only allowed edges. Every mutating method on <see cref="Payment"/> asks this table first,
/// so an illegal move (refund before capture, void after capture, a second 3DS result) is a domain error.
/// </summary>
public static class PaymentStateMachine
{
    private static readonly IReadOnlyDictionary<PaymentStatus, PaymentStatus[]> Edges =
        new Dictionary<PaymentStatus, PaymentStatus[]>
        {
            [PaymentStatus.Created] = [PaymentStatus.Pending3DS, PaymentStatus.Failed],
            [PaymentStatus.Pending3DS] = [PaymentStatus.Authorized, PaymentStatus.Failed],
            [PaymentStatus.Authorized] = [PaymentStatus.Captured, PaymentStatus.Voided],
            [PaymentStatus.Captured] = [PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded],
            [PaymentStatus.PartiallyRefunded] = [PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded],
            [PaymentStatus.Failed] = [],
            [PaymentStatus.Refunded] = [],
            [PaymentStatus.Voided] = []
        };

    public static bool CanTransition(PaymentStatus from, PaymentStatus to) =>
        Edges.TryGetValue(from, out var targets) && targets.Contains(to);

    public static bool IsTerminal(PaymentStatus status) =>
        Edges.TryGetValue(status, out var targets) && targets.Length == 0;

    public static void Ensure(PaymentStatus from, PaymentStatus to)
    {
        if (!CanTransition(from, to))
            throw new PaymentStateException(from, to);
    }
}

public sealed class PaymentStateException : InvalidOperationException
{
    public PaymentStateException(PaymentStatus from, PaymentStatus to)
        : base($"Payment cannot move from {from} to {to}.")
    {
        From = from;
        To = to;
    }

    public PaymentStateException(string message)
        : base(message)
    {
    }

    public PaymentStatus? From { get; }

    public PaymentStatus? To { get; }
}
