using System.Net;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class SocialLoginTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public SocialLoginTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task External_login_without_secrets_explains_not_configured()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });
        var login = await client.GetStringAsync("/giris");
        var token = AntiforgeryTestHelper.GetToken(login);

        var response = await client.PostAsync("/Account/ExternalLogin", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["provider"] = "Google",
            ["__RequestVerificationToken"] = token
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        html.Should().Contain("yapılandırılmadı");
        html.Should().Contain("Google");
    }

    [Fact]
    public async Task Apple_login_without_secrets_explains_not_configured()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });
        var login = await client.GetStringAsync("/kayit");
        var token = AntiforgeryTestHelper.GetToken(login);

        var response = await client.PostAsync("/Account/ExternalLogin", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["provider"] = "Apple",
            ["__RequestVerificationToken"] = token
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        html.Should().Contain("yapılandırılmadı");
        html.Should().Contain("Apple");
    }

    [Fact]
    public async Task External_callback_without_ticket_does_not_500()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Account/ExternalLogin?handler=Callback");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        html.Should().Contain("tamamlanamadı");
    }
}
