using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClearPay.Domain.Identity;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Identity;
using FluentAssertions;

namespace ClearPay.Tests;

/// <summary>
/// Web cookie + Flutter JWT Android share one SQL ledger (T-100).
/// Does not add a 10th screen or a second cash box.
/// </summary>
public sealed class DualSurfaceSmokeTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public DualSurfaceSmokeTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Wallet_entity_has_no_balance_column()
    {
        typeof(Wallet).GetProperty("Balance").Should().BeNull();
    }

    [Fact]
    public async Task Jwt_token_then_wallet_is_the_flutter_android_contract()
    {
        var client = _factory.CreateClient();
        var tokenPost = await client.PostAsJsonAsync("/api/token", new
        {
            email = IdentitySeeder.DevelopmentAdminEmail,
            password = "Deneme123",
            accountKind = AccountKinds.Bireysel
        });
        tokenPost.StatusCode.Should().Be(HttpStatusCode.OK);
        using var tokenDoc = JsonDocument.Parse(await tokenPost.Content.ReadAsStringAsync());
        tokenDoc.RootElement.TryGetProperty("balance", out _).Should().BeFalse();
        tokenDoc.RootElement.GetProperty("token_type").GetString().Should().Be("Bearer");
        var jwt = tokenDoc.RootElement.GetProperty("access_token").GetString();
        jwt.Should().NotBeNullOrEmpty();

        using var walletRequest = new HttpRequestMessage(HttpMethod.Get, "/api/wallet");
        walletRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var wallet = await client.SendAsync(walletRequest);
        wallet.StatusCode.Should().Be(HttpStatusCode.OK);
        using var walletDoc = JsonDocument.Parse(await wallet.Content.ReadAsStringAsync());
        walletDoc.RootElement.TryGetProperty("balance", out var balance).Should().BeTrue();
        balance.ValueKind.Should().Be(JsonValueKind.Number);

        using var hubRequest = new HttpRequestMessage(HttpMethod.Post, "/hubs/wallet/negotiate?negotiateVersion=1");
        hubRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var negotiate = await client.SendAsync(hubRequest);
        negotiate.StatusCode.Should().Be(HttpStatusCode.OK);
        (await negotiate.Content.ReadAsStringAsync()).Should().Contain("connectionToken");
    }

    [Fact]
    public async Task Cookie_demo_tc_login_reaches_overview_without_reload_script()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var loginHtml = await client.GetStringAsync("/giris");
        loginHtml.Should().Contain("TC (demo)");
        loginHtml.Should().Contain(DemoTc.AdminNationalId);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Tc"] = DemoTc.AdminNationalId,
            ["Input.Password"] = "Deneme123",
            ["__RequestVerificationToken"] = AntiforgeryTestHelper.GetToken(loginHtml)
        }));
        post.EnsureSuccessStatusCode();
        var overview = await post.Content.ReadAsStringAsync();
        overview.Should().Contain("Özet");
        overview.Should().Contain("masthead");
        overview.Should().Contain("Kartlarım");
        overview.Should().NotContain("name=\"Input.Tc\"");
        overview.Should().NotContain("UPDATE Balance");

        var js = await client.GetStringAsync("/js/site.js");
        js.Should().NotContain("location.reload");
        js.Should().Contain("/hubs/wallet");
        js.Should().Contain("WalletChanged");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/kartlar")]
    [InlineData("/hareketler")]
    [InlineData("/admin")]
    public async Task Spec_screens_redirect_anonymous_to_giris(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/giris");
    }
}
