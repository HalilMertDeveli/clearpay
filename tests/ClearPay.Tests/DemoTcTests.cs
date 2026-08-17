using ClearPay.Domain.Identity;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class DemoTcTests
{
    [Fact]
    public void ResolveEmail_maps_only_the_documented_seed()
    {
        DemoTc.ResolveEmail("10000000146").Should().Be(DemoTc.AdminEmail);
        DemoTc.ResolveEmail("100-000-00146").Should().Be(DemoTc.AdminEmail);
        DemoTc.ResolveEmail("12345678901").Should().BeNull();
        DemoTc.ResolveEmail(null).Should().BeNull();
    }
}
