namespace ClearPay.Application.Wallets;

/// <summary>DTO for the özet screen. Web binds this; it does not open SQL.</summary>
public sealed record WalletSummary(
    Guid WalletId,
    string UserId,
    decimal Balance,
    decimal MonthOutgoing,
    decimal MonthIncoming,
    bool IsFrozen,
    IReadOnlyList<WalletMovement> LastMovements);
