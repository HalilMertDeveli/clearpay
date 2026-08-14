using ClearPay.Domain.Ledger;
using FluentAssertions;

namespace ClearPay.Tests;

/// <summary>TASK-13: money rules that interviews ask for. No HTTP here — see TransferApiTests for 409.</summary>
public sealed class LedgerInvariantTests
{
    [Fact]
    public void Wallet_type_has_no_balance_property()
    {
        typeof(Wallet).GetProperty("Balance").Should().BeNull();
        typeof(Wallet).GetFields().Should().NotContain(f => f.Name == "Balance");
    }

    [Fact]
    public void Dual_entry_pair_nets_to_zero_across_both_wallets()
    {
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();
        var (debit, credit) = LedgerPair.Create(from, to, 12.34m, Guid.NewGuid(), LedgerEntryKind.Transfer);
        var rows = new[] { debit, credit };

        (debit.Amount + credit.Amount).Should().Be(0m);
        LedgerPair.NetOf(rows, from).Should().Be(-12.34m);
        LedgerPair.NetOf(rows, to).Should().Be(12.34m);
        (LedgerPair.NetOf(rows, from) + LedgerPair.NetOf(rows, to)).Should().Be(0m);
        LedgerPair.WouldGoNegative(0m, debit.Amount).Should().BeTrue();
        LedgerPair.WouldGoNegative(12.34m, debit.Amount).Should().BeFalse();
    }

    [Fact]
    public void Frozen_wallet_cannot_debit()
    {
        var wallet = new Wallet { IsFrozen = true };
        wallet.CanDebit.Should().BeFalse();
        Action act = wallet.EnsureCanDebit;
        act.Should().Throw<InvalidOperationException>();
    }
}
