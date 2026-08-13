using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClearPay.Tests;

/// <summary>
/// TASK-02 smoke plus TASK-03 auth when Coder lands login.
/// Does not touch PlaceholderPagesTests (Coder). Login 404 → skip auth asserts.
/// </summary>
public sealed class AuthOrUiTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public AuthOrUiTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_returns_200()
    {
        var client = CreateClient(allowAutoRedirect: false);

        var response = await client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ClearPay");
    }

    [Fact]
    public async Task Layout_has_spec_menu_and_no_admin()
    {
        var client = CreateClient(allowAutoRedirect: false);
        if (await LoginPageExistsAsync(client))
        {
            return;
        }

        var html = await client.GetStringAsync("/");

        html.Should().Contain("Özet");
        html.Should().Contain("Havale");
        html.Should().Contain("Yükle / Çek");
        html.Should().Contain("Hareketler");
        html.Should().NotContain(">Admin<");
        html.Should().Contain("Havale gönder");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/hareketler")]
    public async Task Menu_routes_are_200_until_login_then_redirect(string path)
    {
        var client = CreateClient(allowAutoRedirect: false);
        var loginExists = await LoginPageExistsAsync(client);

        var response = await client.GetAsync(path);

        if (loginExists)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain("/giris");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Login_and_register_pages_when_identity_is_in()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var loginHtml = await client.GetStringAsync("/Account/Login");
        loginHtml.Should().Contain("E-posta");
        loginHtml.Should().Contain("Şifre");
        loginHtml.Should().Contain("Hesap oluştur");

        var register = await client.GetAsync("/Account/Register");
        register.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerHtml = await register.Content.ReadAsStringAsync();
        registerHtml.Should().Contain("Ad");
        registerHtml.Should().Contain("E-posta");
        registerHtml.Should().Contain("Şifre tekrar");
    }

    [Fact]
    public async Task Register_shows_empty_wallet_when_identity_is_in()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var email = $"ayse.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetAsync("/Account/Register");
        registerGet.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = AuthFormToken.Get(await registerGet.Content.ReadAsStringAsync());

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
        wallet.Should().Contain("Bu ay giden");
        wallet.Should().Contain("Bu ay gelen");
        wallet.Should().Contain("Yükle / Çek");
        wallet.Should().NotContain(">Admin<");
    }

    [Fact]
    public async Task Bad_login_stays_on_form_when_identity_is_in()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var loginHtml = await client.GetStringAsync("/Account/Login");
        var token = AuthFormToken.Get(loginHtml);

        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = "yok@clearpay.test",
            ["Input.Password"] = "Yanlis123",
            ["__RequestVerificationToken"] = token
        }));

        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await post.Content.ReadAsStringAsync();
        html.Should().Contain("E-posta");
        html.Should().Contain("Şifre");
    }

    [Fact(Skip = "TASK-06: Idempotency-Key 409 henüz yok; ledger gelince aç.")]
    public void Duplicate_transfer_returns_409()
    {
    }

    private HttpClient CreateClient(bool allowAutoRedirect = true)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"clearpay-identity-{Guid.NewGuid():N}.db");
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Identity", $"Data Source={dbPath}");
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
            HandleCookies = true
        });
    }

    private static async Task<bool> LoginPageExistsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/Account/Login");
        return response.StatusCode == HttpStatusCode.OK;
    }

    private static class AuthFormToken
    {
        public static string Get(string html)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                html,
                @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = System.Text.RegularExpressions.Regex.Match(
                    html,
                    @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            match.Success.Should().BeTrue("the form should include an antiforgery token");
            return match.Groups[1].Value;
        }
    }
}
