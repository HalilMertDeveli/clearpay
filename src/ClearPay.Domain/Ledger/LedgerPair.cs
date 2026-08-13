namespace ClearPay.Domain.Ledger;

/// <summary>
/// Çift kayıt kuralı: her harekette bir debit (−) ve bir credit (+) satırı.
/// Tutarlar toplamı sıfır. Bakiye = cüzdanın ledger net’i.
/// Yasak: <c>wallet.Balance -= amount; SaveChanges();</c> — audit’siz, tek taraflı, iadesiz.
/// </summary>
public static class LedgerPair
{
    /// <summary>
    /// Builds the +/− pair. <paramref name="amount"/> must be positive.
    /// Debit wallet is charged; credit wallet receives. Same PairId and CorrelationId.
    /// </summary>
    public static (LedgerEntry Debit, LedgerEntry Credit) Create(
        Guid debitWalletId,
        Guid creditWalletId,
        decimal amount,
        Guid correlationId,
        LedgerEntryKind kind,
        Guid? transferId = null,
        string? description = null,
        DateTimeOffset? at = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0m);
        if (debitWalletId == creditWalletId)
            throw new InvalidOperationException("Ledger pair requires two different wallets.");

        var when = at ?? DateTimeOffset.UtcNow;
        var pairId = Guid.NewGuid();

        var debit = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            WalletId = debitWalletId,
            Amount = decimal.Negate(amount),
            PairId = pairId,
            CorrelationId = correlationId,
            TransferId = transferId,
            Kind = kind,
            Description = description,
            CreatedAt = when
        };

        var credit = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            WalletId = creditWalletId,
            Amount = amount,
            PairId = pairId,
            CorrelationId = correlationId,
            TransferId = transferId,
            Kind = kind,
            Description = description,
            CreatedAt = when
        };

        EnsureBalanced(debit, credit);
        return (debit, credit);
    }

    /// <summary>Iade = ters çift (eski credit cüzdanı debit olur). Elle bakiye düzeltmesi yok.</summary>
    public static (LedgerEntry Debit, LedgerEntry Credit) CreateRefund(
        LedgerEntry originalDebit,
        LedgerEntry originalCredit,
        Guid correlationId,
        DateTimeOffset? at = null)
    {
        EnsureBalanced(originalDebit, originalCredit);
        var amount = originalCredit.Amount;
        return Create(
            debitWalletId: originalCredit.WalletId,
            creditWalletId: originalDebit.WalletId,
            amount,
            correlationId,
            LedgerEntryKind.Refund,
            transferId: null,
            description: originalCredit.Description,
            at);
    }

    public static void EnsureBalanced(LedgerEntry left, LedgerEntry right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.PairId != right.PairId || left.PairId == Guid.Empty)
            throw new InvalidOperationException("Pair rows must share a non-empty PairId.");

        if (left.CorrelationId != right.CorrelationId || left.CorrelationId == Guid.Empty)
            throw new InvalidOperationException("Pair rows must share a non-empty CorrelationId.");

        if (left.WalletId == right.WalletId)
            throw new InvalidOperationException("Pair wallets must differ.");

        if (left.Amount + right.Amount != 0m)
            throw new InvalidOperationException("Pair amounts must sum to zero.");

        var hasDebit = left.IsDebit || right.IsDebit;
        var hasCredit = left.IsCredit || right.IsCredit;
        if (!hasDebit || !hasCredit)
            throw new InvalidOperationException("Pair must include one debit (−) and one credit (+).");
    }

    /// <summary>Negatif bakiye yok: currentNet + debitAmount (debitAmount &lt; 0) must stay ≥ 0.</summary>
    public static bool WouldGoNegative(decimal currentNet, decimal debitAmount)
    {
        if (debitAmount >= 0m)
            throw new ArgumentOutOfRangeException(nameof(debitAmount), "Debit amount must be negative.");

        return currentNet + debitAmount < 0m;
    }
}
