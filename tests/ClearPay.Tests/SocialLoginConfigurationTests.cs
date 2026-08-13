using ClearPay.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ClearPay.Tests;

public sealed class SocialLoginConfigurationTests
{
    [Fact]
    public void Empty_placeholders_are_not_configured()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Google:ClientId"] = "",
            ["Authentication:Google:ClientSecret"] = "",
            ["Authentication:Apple:ClientId"] = ""
        }).Build();

        SocialLoginConfiguration.IsGoogleConfigured(config).Should().BeFalse();
        SocialLoginConfiguration.IsAppleConfigured(config).Should().BeFalse();
        SocialLoginConfiguration.IsConfigured(config, "Google").Should().BeFalse();
    }

    [Fact]
    public void Reads_env_style_google_keys()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Google:ClientId"] = "id.apps.googleusercontent.com",
            ["Google:ClientSecret"] = "secret"
        }).Build();

        SocialLoginConfiguration.IsGoogleConfigured(config).Should().BeTrue();
    }

    [Fact]
    public void Reads_apple_keys()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Apple:ClientId"] = "com.clearpay.demo",
            ["Authentication:Apple:TeamId"] = "TEAMID",
            ["Authentication:Apple:KeyId"] = "KEYID",
            ["Authentication:Apple:PrivateKey"] = "-----BEGIN PRIVATE KEY-----\nMII\n-----END PRIVATE KEY-----"
        }).Build();

        SocialLoginConfiguration.IsAppleConfigured(config).Should().BeTrue();
        SocialLoginConfiguration.IsKnownProvider("Apple").Should().BeTrue();
        SocialLoginConfiguration.IsKnownProvider("Facebook").Should().BeFalse();
    }
}
