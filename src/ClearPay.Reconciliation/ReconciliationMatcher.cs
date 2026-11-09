namespace ClearPay.Reconciliation;

public static class MatchStatus
{
    public const string Matched = "Matched";
    public const string OnlyAtBank = "OnlyAtBank";
    public const string OnlyInClearPay = "OnlyInClearPay";
    public const string AmountMismatch = "AmountMismatch";

    /// <summary>Wallet ledger day where a kind's +/- pairs do not net to zero (from the T-SQL team's SP).</summary>
    public const string LedgerImbalance = "LedgerImbalance";
}

public sealed record MatchResult(
    string Provider,
    string Kind,
    string Reference,
    string Status,
    decimal? BankAmount,
    decimal? ClearPayAmount,
    string Currency);

/// <summary>
/// Pure matching, no I/O, so every rule has a unit test. Key = (provider, record type, reference).
/// Same key on both sides: equal amount and currency → Matched, otherwise AmountMismatch.
/// If a key appears twice on one side, the amounts are summed (a split settlement still has to add up).
/// </summary>
public static class ReconciliationMatcher
{
    public static IReadOnlyList<MatchResult> Match(IEnumerable<SettlementLine> bank, IEnumerable<SettlementLine> ours)
    {
        var bankByKey = Group(bank);
        var oursByKey = Group(ours);
        var results = new List<MatchResult>();

        foreach (var (key, b) in bankByKey)
        {
            if (!oursByKey.TryGetValue(key, out var o))
            {
                results.Add(new MatchResult(key.Provider, key.Kind, key.Reference, MatchStatus.OnlyAtBank, b.Amount, null, b.Currency));
                continue;
            }

            var status = b.Amount == o.Amount && b.Currency == o.Currency ? MatchStatus.Matched : MatchStatus.AmountMismatch;
            results.Add(new MatchResult(key.Provider, key.Kind, key.Reference, status, b.Amount, o.Amount, o.Currency));
        }

        foreach (var (key, o) in oursByKey)
        {
            if (!bankByKey.ContainsKey(key))
                results.Add(new MatchResult(key.Provider, key.Kind, key.Reference, MatchStatus.OnlyInClearPay, null, o.Amount, o.Currency));
        }

        return results
            .OrderBy(r => r.Provider, StringComparer.Ordinal)
            .ThenBy(r => r.Status == MatchStatus.Matched ? 1 : 0)
            .ThenBy(r => r.Reference, StringComparer.Ordinal)
            .ToList();
    }

    private static Dictionary<(string Provider, string Kind, string Reference), (decimal Amount, string Currency)> Group(
        IEnumerable<SettlementLine> lines) =>
        lines
            .GroupBy(l => (l.Provider, l.Kind, Reference: l.Reference.ToLowerInvariant()))
            .ToDictionary(g => g.Key, g => (g.Sum(l => l.Amount), g.First().Currency));
}
