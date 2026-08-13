using ClearPay.Domain.Ledger;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class LedgerPairTests
{
    [Fact]
    public void Create_writes_debit_and_credit_that_sum_to_zero()
    {
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();
        var correlation = Guid.NewGuid();
        var transferId = Guid.NewGuid();

        var (debit, credit) = LedgerPair.Create(
            from,
            to,
            25.50m,
            correlation,
            LedgerEntryKind.Transfer,
            transferId,
            "Havale");

        debit.Amount.Should().Be(-25.50m);
        credit.Amount.Should().Be(25.50m);
        (debit.Amount + credit.Amount).Should().Be(0m);
        debit.IsDebit.Should().BeTrue();
        credit.IsCredit.Should().BeTrue();
        debit.WalletId.Should().Be(from);
        credit.WalletId.Should().Be(to);
        debit.PairId.Should().Be(credit.PairId).And.NotBe(Guid.Empty);
        debit.CorrelationId.Should().Be(correlation);
        credit.CorrelationId.Should().Be(correlation);
        debit.Kind.Should().Be(LedgerEntryKind.Transfer);
        debit.TransferId.Should().Be(transferId);
        LedgerPair.EnsureBalanced(debit, credit);
    }

    [Fact]
    public void NetOf_is_signed_sum_for_one_wallet()
    {
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();
        var (debit, credit) = LedgerPair.Create(from, to, 40m, Guid.NewGuid(), LedgerEntryKind.Transfer);
        var (backDebit, backCredit) = LedgerPair.Create(to, from, 15m, Guid.NewGuid(), LedgerEntryKind.Transfer);
        var rows = new[] { debit, credit, backDebit, backCredit };

        LedgerPair.NetOf(rows, from).Should().Be(-25m);
        LedgerPair.NetOf(rows, to).Should().Be(25m);
        (LedgerPair.NetOf(rows, from) + LedgerPair.NetOf(rows, to)).Should().Be(0m);
        LedgerPair.NetOf(rows, Guid.NewGuid()).Should().Be(0m);
        LedgerPair.NetOf([], from).Should().Be(0m);
    }

    [Fact]
    public void Create_rejects_non_positive_amount_and_same_wallet()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var cid = Guid.NewGuid();

        var zero = () => LedgerPair.Create(a, b, 0m, cid, LedgerEntryKind.Transfer);
        var negative = () => LedgerPair.Create(a, b, -1m, cid, LedgerEntryKind.Transfer);
        var self = () => LedgerPair.Create(a, a, 10m, cid, LedgerEntryKind.Transfer);

        zero.Should().Throw<ArgumentOutOfRangeException>();
        negative.Should().Throw<ArgumentOutOfRangeException>();
        self.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateRefund_reverses_wallets_without_updating_balance()
    {
        var from = Guid.NewGuid();
        var to = Guid.NewGuid();
        var (debit, credit) = LedgerPair.Create(from, to, 10m, Guid.NewGuid(), LedgerEntryKind.Transfer);
        var refundCorrelation = Guid.NewGuid();

        var (refundDebit, refundCredit) = LedgerPair.CreateRefund(debit, credit, refundCorrelation);

        refundDebit.WalletId.Should().Be(to);
        refundCredit.WalletId.Should().Be(from);
        refundDebit.Amount.Should().Be(-10m);
        refundCredit.Amount.Should().Be(10m);
        refundDebit.Kind.Should().Be(LedgerEntryKind.Refund);
        refundDebit.CorrelationId.Should().Be(refundCorrelation);
        (refundDebit.Amount + refundCredit.Amount).Should().Be(0m);
        LedgerPair.NetOf([refundDebit, refundCredit], from).Should().Be(10m);
        LedgerPair.NetOf([refundDebit, refundCredit], to).Should().Be(-10m);
    }

    [Fact]
    public void WouldGoNegative_blocks_debit_past_zero()
    {
        LedgerPair.WouldGoNegative(currentNet: 10m, debitAmount: -10m).Should().BeFalse();
        LedgerPair.WouldGoNegative(currentNet: 10m, debitAmount: -10.01m).Should().BeTrue();
        LedgerPair.WouldGoNegative(currentNet: 0m, debitAmount: -0.01m).Should().BeTrue();

        var creditAsDebit = () => LedgerPair.WouldGoNegative(10m, 5m);
        creditAsDebit.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Frozen_wallet_cannot_debit()
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            IsFrozen = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        wallet.CanDebit.Should().BeFalse();
        var frozenDebit = () => wallet.EnsureCanDebit();
        frozenDebit.Should().Throw<InvalidOperationException>();
        new Wallet { UserId = "user-2" }.CanDebit.Should().BeTrue();
        new Wallet { UserId = "user-2" }.EnsureCanDebit();
    }

    [Fact]
    public void Money_transaction_requires_pair_and_side_rows()
    {
        MoneyTransaction.RequiredInserts.Should().Equal(
            "LedgerEntry#debit",
            "LedgerEntry#credit",
            nameof(Transfer),
            nameof(IdempotencyRecord),
            nameof(AuditLog),
            nameof(OutboxMessage));
    }

    [Fact]
    public void Ledger_pocos_construct()
    {
        var correlation = Guid.NewGuid();
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            FromWalletId = Guid.NewGuid(),
            ToWalletId = Guid.NewGuid(),
            Amount = 1m,
            Status = TransferStatus.Completed,
            CorrelationId = correlation,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var idempotency = new IdempotencyRecord
        {
            Key = "k1",
            Scope = "transfer",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = "user-1",
            Action = "transfer",
            CorrelationId = correlation,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TransferCompleted",
            Payload = "{}",
            CorrelationId = correlation,
            OccurredAt = DateTimeOffset.UtcNow
        };

        transfer.Amount.Should().BePositive();
        idempotency.Key.Should().NotBeNullOrEmpty();
        audit.CorrelationId.Should().Be(correlation);
        outbox.ProcessedAt.Should().BeNull();
        outbox.Status.Should().Be(OutboxStatus.Pending);
        LedgerSchema.AmountScale.Should().Be(2);
        LedgerSchema.IdempotencyKeyUnique.Should().Be("UX_IdempotencyRecord_Key");
    }
}
