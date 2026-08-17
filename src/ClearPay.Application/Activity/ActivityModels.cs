namespace ClearPay.Application.Activity;

public sealed record ActivityItem(
    DateTimeOffset At,
    Guid CorrelationId,
    string Kind,
    string Counterparty,
    decimal SignedAmount,
    string Status);

public sealed record ActivityPage(
    IReadOnlyList<ActivityItem> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ReceiptDto(
    Guid CorrelationId,
    DateTimeOffset At,
    string Kind,
    decimal Amount,
    string DebitParty,
    string CreditParty,
    string? Description)
{
    /// <summary>Funding account hint (****1234) when the ledger description is a linked instrument.</summary>
    public string? InstrumentHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Description))
                return null;
            var text = Description.Trim();
            return text.Contains("****", StringComparison.Ordinal) ? text : null;
        }
    }
}
