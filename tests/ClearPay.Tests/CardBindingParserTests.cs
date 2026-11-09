using ClearPay.Application.Funding;
using ClearPay.Domain.Ledger;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class CardBindingParserTests
{
    [Fact]
    public void Visa_demo_pan_yields_last4_and_scheme_not_pan()
    {
        CardBindingParser.TryParse("4111 1111 1111 1111", "Yapı Kredi", out var last4, out var scheme, out var nickname)
            .Should().BeTrue();
        last4.Should().Be("1111");
        scheme.Should().Be(CardNetwork.Visa);
        nickname.Should().Be("Yapı Kredi");
    }

    [Fact]
    public void Troy_bin_9792_is_detected()
    {
        CardBindingParser.TryParse("9792123412341234", "Troy Demo", out var last4, out var scheme, out _)
            .Should().BeTrue();
        last4.Should().Be("1234");
        scheme.Should().Be(CardNetwork.Troy);
    }

    [Fact]
    public void Mastercard_bin_is_detected()
    {
        CardBindingParser.TryParse("5555555555554444", null, out var last4, out var scheme, out var nickname)
            .Should().BeTrue();
        last4.Should().Be("4444");
        scheme.Should().Be(CardNetwork.Mastercard);
        nickname.Should().Be("ClearPay Demo");
    }

    [Fact]
    public void Mastercard_2_series_bin_is_detected()
    {
        CardBindingParser.TryParse("2223003122003222", "MC Demo", out var last4, out var scheme, out _)
            .Should().BeTrue();
        last4.Should().Be("3222");
        scheme.Should().Be(CardNetwork.Mastercard);
    }

    [Fact]
    public void Last4_only_is_unknown_scheme()
    {
        CardBindingParser.TryParse("4242", "Maas", out var last4, out var scheme, out _)
            .Should().BeTrue();
        last4.Should().Be("4242");
        scheme.Should().Be(CardNetwork.Unknown);
    }

    [Fact]
    public void Rejects_too_short_or_empty()
    {
        CardBindingParser.TryParse("12", "x", out _, out _, out _).Should().BeFalse();
        CardBindingParser.TryParse("", "x", out _, out _, out _).Should().BeFalse();
    }
}
