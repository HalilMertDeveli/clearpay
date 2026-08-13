using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class WalletFlowTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public WalletFlowTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_signs_in_and_shows_empty_wallet()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var email = $"ayse.{Guid.NewGuid():N}@clearpay.test";

        var registerGet = await client.GetAsync("/Account/Register");
        registerGet.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerHtml = await registerGet.Content.ReadAsStringAsync();
        var token = AntiforgeryTestHelper.GetToken(registerHtml);

        var registerPost = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Ayşe Yılmaz",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        registerPost.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await registerPost.Content.ReadAsStringAsync();
        wallet.Should().Contain("0,00 ₺");
        wallet.Should().Contain("Henüz hareket yok.");
        wallet.Should().Contain("Havale gönder");
        wallet.Should().Contain("Yükle");
        wallet.Should().Contain("Çek");
        wallet.Should().Contain("Özet");
    }

    [Fact]
    public async Task Login_after_register_returns_wallet_summary()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var email = $"ali.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, email, "Ali Kaya");

        await client.PostAsync("/Account/Logout", await FormWithTokenAsync(client, "/"));

        var loginGet = await client.GetAsync("/Account/Login");
        var loginHtml = await loginGet.Content.ReadAsStringAsync();
        var token = AntiforgeryTestHelper.GetToken(loginHtml);

        var loginPost = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        loginPost.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await loginPost.Content.ReadAsStringAsync();
        wallet.Should().Contain("0,00 ₺");
        wallet.Should().Contain("Bu ay giden");
        wallet.Should().Contain("Bu ay gelen");
    }

    private static async Task RegisterAsync(HttpClient client, string email, string name)
    {
        var page = await client.GetStringAsync("/Account/Register");
        var token = AntiforgeryTestHelper.GetToken(page);
        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = name,
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));
        response.EnsureSuccessStatusCode();
    }

    private static async Task<FormUrlEncodedContent> FormWithTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var token = AntiforgeryTestHelper.GetToken(html);
        return new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });
    }
}

internal static class AntiforgeryTestHelper
{
    public static string GetToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(
                html,
                @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
                RegexOptions.IgnoreCase);
        }

        match.Success.Should().BeTrue("the form should include an antiforgery token");
        return match.Groups[1].Value;
    }
}
