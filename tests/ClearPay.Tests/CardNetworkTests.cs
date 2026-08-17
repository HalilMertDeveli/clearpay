using ClearPay.Domain.Ledger;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class CardNetworkTests
{
    [Theory]
    [InlineData("4", "Visa")]
    [InlineData("4111111111111111", "Visa")]
    [InlineData("51", "Mastercard")]
    [InlineData("5555555555554444", "Mastercard")]
    [InlineData("2221", "Mastercard")]
    [InlineData("2223003122003222", "Mastercard")]
    [InlineData("2720", "Mastercard")]
    [InlineData("9792", "Troy")]
    [InlineData("9792123412341234", "Troy")]
    [InlineData("34", "Unknown")]
    [InlineData("", "Unknown")]
    public void Detect_uses_iso_iin_prefixes(string digits, string scheme)
    {
        CardNetwork.Detect(digits).Should().Be(scheme);
    }
}
