using ClearPay.Application.Payments;
using ClearPay.Infrastructure.Payments.Acquirers;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class AcquirerAdapterTests
{
    private static readonly IAcquirerAdapter[] All =
        [new MockBankAAdapter(), new MockBankBAdapter(), new MockPspCAdapter()];

    [Theory]
    [InlineData("00", AcquirerResponseCode.Approved)]
    [InlineData("51", AcquirerResponseCode.InsufficientFunds)]
    [InlineData("05", AcquirerResponseCode.Declined)]
    [InlineData("14", AcquirerResponseCode.InvalidCard)]
    [InlineData("91", AcquirerResponseCode.IssuerUnavailable)]
    [InlineData("XX", AcquirerResponseCode.Unknown)]
    public void Bank_a_iso8583_codes_are_normalized(string raw, AcquirerResponseCode expected)
    {
        var result = MockBankAAdapter.Normalize(raw, "REF");
        result.Code.Should().Be(expected);
        result.RawCode.Should().Be(raw);
    }

    [Fact]
    public void Bank_b_xml_is_parsed_and_garbage_is_never_approved()
    {
        MockBankBAdapter.Normalize(
                "<PosResponse><Result>Error</Result><ErrorCode>1051</ErrorCode><ErrorMessage>x</ErrorMessage></PosResponse>")
            .Code.Should().Be(AcquirerResponseCode.InsufficientFunds);
        MockBankBAdapter.Normalize(
                "<PosResponse><Result>Approved</Result><ErrorCode>0</ErrorCode><HostRefNum>B123</HostRefNum></PosResponse>")
            .Should().Match<AcquirerResult>(r => r.IsApproved && r.Reference == "B123");
        MockBankBAdapter.Normalize("<not-closed").Code.Should().Be(AcquirerResponseCode.Unknown);
        MockBankBAdapter.Normalize("<PosResponse><Result>Approved</Result><ErrorCode>7</ErrorCode></PosResponse>")
            .IsApproved.Should().BeFalse("Approved text with a non-zero code is inconsistent and must not pass");
    }

    [Fact]
    public void Psp_c_json_decline_codes_are_normalized()
    {
        MockPspCAdapter.Normalize("""{"status":"failed","decline_code":"insufficient_funds"}""")
            .Code.Should().Be(AcquirerResponseCode.InsufficientFunds);
        MockPspCAdapter.Normalize("""{"status":"failed","decline_code":"incorrect_number"}""")
            .Code.Should().Be(AcquirerResponseCode.InvalidCard);
        MockPspCAdapter.Normalize("""{"status":"succeeded","id":"ch_1"}""")
            .Should().Match<AcquirerResult>(r => r.IsApproved && r.Reference == "ch_1");
        MockPspCAdapter.Normalize("{oops").Code.Should().Be(AcquirerResponseCode.Unknown);
    }

    [Theory]
    [InlineData(100.00, "1111", AcquirerResponseCode.Approved)]
    [InlineData(100.51, "1111", AcquirerResponseCode.InsufficientFunds)]
    [InlineData(100.05, "1111", AcquirerResponseCode.Declined)]
    [InlineData(100.91, "1111", AcquirerResponseCode.IssuerUnavailable)]
    [InlineData(100.00, "0000", AcquirerResponseCode.InvalidCard)]
    public async Task Same_scenario_gives_same_normalized_code_on_every_adapter(
        double amount,
        string last4,
        AcquirerResponseCode expected)
    {
        var request = new AcquirerRequest(Guid.NewGuid(), (decimal)amount, "TRY", "411111", last4, "tx", null);
        foreach (var adapter in All)
        {
            var result = await adapter.AuthorizeAsync(request);
            result.Code.Should().Be(expected, adapter.Name);
            if (expected == AcquirerResponseCode.Approved)
                result.Reference.Should().NotBeNullOrEmpty(adapter.Name);
        }
    }

    [Theory]
    [InlineData("411111", MockBankAAdapter.ProviderName)]
    [InlineData("555555", MockBankBAdapter.ProviderName)]
    [InlineData("979203", MockPspCAdapter.ProviderName)]
    [InlineData("979100", MockBankAAdapter.ProviderName)]
    [InlineData("601100", MockBankAAdapter.ProviderName)]
    public void Router_uses_longest_bin_prefix_then_default(string bin, string expected)
    {
        var router = new AcquirerRouter(All, new PaymentRoutingOptions
        {
            DefaultProvider = MockBankAAdapter.ProviderName,
            BinRoutes = new Dictionary<string, string>
            {
                ["4"] = MockBankAAdapter.ProviderName,
                ["5"] = MockBankBAdapter.ProviderName,
                ["9"] = MockBankAAdapter.ProviderName,
                ["9792"] = MockPspCAdapter.ProviderName
            }
        });

        router.ForBin(bin).Name.Should().Be(expected);
    }

    [Fact]
    public void Router_refuses_config_that_points_at_a_missing_adapter()
    {
        var act = () => new AcquirerRouter(All, new PaymentRoutingOptions
        {
            DefaultProvider = MockBankAAdapter.ProviderName,
            BinRoutes = new Dictionary<string, string> { ["4"] = "RealBank" }
        });
        act.Should().Throw<InvalidOperationException>().WithMessage("*RealBank*");
    }

    [Fact]
    public void Hmac_signature_binds_payment_transaction_result_and_amount()
    {
        var signer = new ThreeDsSigner(new ThreeDsOptions { AcsSecret = "test-secret" });
        var id = Guid.NewGuid();
        var sig = signer.Sign(id, "tx1", "Y", 10.00m);

        signer.Verify(id, "tx1", "Y", 10.00m, sig).Should().BeTrue();
        signer.Verify(id, "tx1", "Y", 10.00m, sig.ToLowerInvariant()).Should().BeTrue();
        signer.Verify(id, "tx1", "N", 10.00m, sig).Should().BeFalse();
        signer.Verify(id, "tx1", "Y", 10.01m, sig).Should().BeFalse();
        signer.Verify(Guid.NewGuid(), "tx1", "Y", 10.00m, sig).Should().BeFalse();
        signer.Verify(id, "tx2", "Y", 10.00m, sig).Should().BeFalse();
        signer.Verify(id, "tx1", "Y", 10.00m, "not-hex").Should().BeFalse();
        signer.Verify(id, "tx1", "Y", 10.00m, null).Should().BeFalse();
        new ThreeDsSigner(new ThreeDsOptions { AcsSecret = "other" })
            .Verify(id, "tx1", "Y", 10.00m, sig).Should().BeFalse();
    }
}
