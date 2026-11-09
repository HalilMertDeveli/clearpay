using ClearPay.Domain.Payments;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class PaymentDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(PaymentStatus.Created, PaymentStatus.Pending3DS, true)]
    [InlineData(PaymentStatus.Pending3DS, PaymentStatus.Authorized, true)]
    [InlineData(PaymentStatus.Pending3DS, PaymentStatus.Failed, true)]
    [InlineData(PaymentStatus.Authorized, PaymentStatus.Captured, true)]
    [InlineData(PaymentStatus.Authorized, PaymentStatus.Voided, true)]
    [InlineData(PaymentStatus.Captured, PaymentStatus.PartiallyRefunded, true)]
    [InlineData(PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded, true)]
    [InlineData(PaymentStatus.Created, PaymentStatus.Authorized, false)]
    [InlineData(PaymentStatus.Pending3DS, PaymentStatus.Captured, false)]
    [InlineData(PaymentStatus.Captured, PaymentStatus.Voided, false)]
    [InlineData(PaymentStatus.Failed, PaymentStatus.Authorized, false)]
    [InlineData(PaymentStatus.Voided, PaymentStatus.Captured, false)]
    [InlineData(PaymentStatus.Refunded, PaymentStatus.PartiallyRefunded, false)]
    public void State_machine_allows_only_listed_edges(PaymentStatus from, PaymentStatus to, bool allowed)
    {
        PaymentStateMachine.CanTransition(from, to).Should().Be(allowed);
    }

    [Fact]
    public void Terminal_states_have_no_exit()
    {
        PaymentStateMachine.IsTerminal(PaymentStatus.Failed).Should().BeTrue();
        PaymentStateMachine.IsTerminal(PaymentStatus.Voided).Should().BeTrue();
        PaymentStateMachine.IsTerminal(PaymentStatus.Refunded).Should().BeTrue();
        PaymentStateMachine.IsTerminal(PaymentStatus.Captured).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.001)]
    public void Create_rejects_non_kurus_amounts(double amount)
    {
        var act = () => NewPayment((decimal)amount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Void_after_capture_is_illegal_refund_before_capture_is_illegal()
    {
        var payment = Authorized(100m);
        var refundEarly = () => payment.ReserveRefund(10m, null, Now);
        refundEarly.Should().Throw<PaymentStateException>();

        payment.Capture(Now);
        var voidLate = () => payment.Void(Now);
        voidLate.Should().Throw<PaymentStateException>();
        payment.Status.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public void Partial_refunds_cannot_exceed_captured_amount()
    {
        var payment = Authorized(100m);
        payment.Capture(Now);

        var first = payment.ReserveRefund(60.50m, "size", Now);
        payment.CompleteRefund(first, "R1", Now);
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.RefundableAmount.Should().Be(39.50m);

        var over = () => payment.ReserveRefund(39.51m, null, Now);
        over.Should().Throw<RefundLimitExceededException>();

        var rest = payment.ReserveRefund(39.50m, null, Now);
        payment.CompleteRefund(rest, "R2", Now);
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAmount.Should().Be(100m);
    }

    [Fact]
    public void Pending_reservation_counts_against_the_limit_and_release_gives_it_back()
    {
        var payment = Authorized(100m);
        payment.Capture(Now);

        var pending = payment.ReserveRefund(80m, null, Now);
        var second = () => payment.ReserveRefund(30m, null, Now);
        second.Should().Throw<RefundLimitExceededException>("80 is reserved while the bank is being called");

        payment.ReleaseRefund(pending, "Declined", Now);
        pending.Status.Should().Be(PaymentRefundStatus.Failed);
        payment.Status.Should().Be(PaymentStatus.Captured);
        payment.RefundableAmount.Should().Be(100m);
    }

    private static Payment NewPayment(decimal amount) =>
        Payment.Create("m1", "o1", amount, "try", "411111", "1111", "MockBankA", "/r", Now);

    private static Payment Authorized(decimal amount)
    {
        var payment = NewPayment(amount);
        payment.Currency.Should().Be("TRY");
        payment.StartThreeDs("tx", Now);
        payment.Authorize("AUTH", Now);
        return payment;
    }
}
