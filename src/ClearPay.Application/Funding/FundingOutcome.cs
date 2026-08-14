namespace ClearPay.Application.Funding;

public sealed record FundingOutcome(
    FundingResultKind Kind,
    Guid CorrelationId,
    string? GatewayReference)
{
    public bool IsSuccess => Kind == FundingResultKind.Created;

    public static FundingOutcome Created(Guid correlationId, string? reference) =>
        new(FundingResultKind.Created, correlationId, reference);

    public static FundingOutcome Fail(FundingResultKind kind, Guid correlationId = default) =>
        new(kind, correlationId, null);
}
