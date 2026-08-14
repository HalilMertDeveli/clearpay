namespace ClearPay.Application.Transfers;

/// <summary>HTTP layer maps these to 201 / 409 / 4xx. No second debit on Replay.</summary>
public enum TransferResultKind
{
    Created = 0,
    Replay = 1,
    KeyPayloadMismatch = 2,
    InsufficientFunds = 3,
    FrozenSender = 4,
    SelfTransfer = 5,
    RecipientNotFound = 6,
    InvalidAmount = 7,
    MissingKey = 8,
    Unavailable = 9
}
