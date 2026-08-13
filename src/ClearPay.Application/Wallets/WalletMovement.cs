namespace ClearPay.Application.Wallets;

public sealed record WalletMovement(
    DateTimeOffset At,
    string Kind,
    decimal Amount,
    Guid CorrelationId);
