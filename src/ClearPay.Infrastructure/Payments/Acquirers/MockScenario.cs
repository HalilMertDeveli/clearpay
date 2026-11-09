namespace ClearPay.Infrastructure.Payments.Acquirers;

/// <summary>
/// Deterministic test triggers shared by every mock bank, so demos and tests can force a decline.
/// The kuruş part of the amount picks the scenario (like many real bank test environments):
/// .51 insufficient funds, .05 do-not-honor, .91 issuer unavailable; card ending 0000 = invalid card.
/// Each adapter then renders the scenario in its own wire format.
/// </summary>
internal enum MockScenario
{
    Approve,
    Decline,
    InsufficientFunds,
    InvalidCard,
    IssuerDown
}

internal static class MockScenarios
{
    public static MockScenario ForAuthorization(decimal amount, string cardLast4)
    {
        if (cardLast4 == "0000")
            return MockScenario.InvalidCard;
        return ByKurus(amount);
    }

    /// <summary>Refund of an amount ending in .05 is declined, so the "release reservation" path is demoable.</summary>
    public static MockScenario ForRefund(decimal refundAmount) =>
        Kurus(refundAmount) == 5 ? MockScenario.Decline : MockScenario.Approve;

    private static MockScenario ByKurus(decimal amount) => Kurus(amount) switch
    {
        51 => MockScenario.InsufficientFunds,
        5 => MockScenario.Decline,
        91 => MockScenario.IssuerDown,
        _ => MockScenario.Approve
    };

    private static int Kurus(decimal amount) => (int)(decimal.Round(amount * 100m) % 100m);
}
