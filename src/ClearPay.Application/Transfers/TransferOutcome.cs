namespace ClearPay.Application.Transfers;

/// <summary><see cref="IsReplay"/> true means the key was seen — caller maps to 409.</summary>
public sealed record TransferOutcome(
    TransferResultKind Kind,
    Guid? TransferId,
    Guid CorrelationId)
{
    public bool IsReplay =>
        Kind is TransferResultKind.Replay or TransferResultKind.KeyPayloadMismatch;

    public bool IsSuccess => Kind == TransferResultKind.Created;

    public static TransferOutcome Created(Guid transferId, Guid correlationId) =>
        new(TransferResultKind.Created, transferId, correlationId);

    public static TransferOutcome Replay(Guid? transferId, Guid correlationId) =>
        new(TransferResultKind.Replay, transferId, correlationId);

    public static TransferOutcome Fail(TransferResultKind kind) =>
        new(kind, null, Guid.Empty);
}
