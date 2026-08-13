using System.Globalization;
using System.Net;
using ClearPay.Web.Presentation;
using FluentAssertions;
using Microsoft.AspNetCore.Localization;

namespace ClearPay.Tests;

public sealed class LocalizationTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public LocalizationTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_defaults_to_turkish_and_shows_language_picker()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/giris");

        html.Should().Contain("lang=\"tr\"");
        html.Should().Contain("Giriş");
        html.Should().Contain("Türkçe");
        html.Should().Contain("English");
        html.Should().Contain("Deutsch");
        html.Should().Contain("Français");
        html.Should().Contain("Demo — yükleme için sahte gateway");
    }

    [Theory]
    [InlineData("en", "Sign in", "lang=\"en\"", "Demo — fake gateway for top-ups")]
    [InlineData("de", "Anmelden", "lang=\"de\"", "Demo — gefälschtes Gateway für Aufladungen")]
    [InlineData("fr", "Connexion", "lang=\"fr\"", "Démo — passerelle fictive pour les recharges")]
    [InlineData("tr", "Giriş", "lang=\"tr\"", "Demo — yükleme için sahte gateway")]
    public async Task Culture_form_switches_login_copy(string culture, string heading, string htmlLang, string disclaimer)
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var login = await client.GetStringAsync("/giris");
        var token = AntiforgeryTestHelper.GetToken(login);

        var post = await client.PostAsync("/culture", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["culture"] = culture,
            ["returnUrl"] = "/giris",
            ["__RequestVerificationToken"] = token
        }));

        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await post.Content.ReadAsStringAsync();
        html.Should().Contain(htmlLang);
        html.Should().Contain(heading);
        html.Should().Contain(disclaimer);
        html.Should().Contain($"value=\"{culture}\"");
    }

    [Fact]
    public async Task English_cookie_formats_wallet_balance_with_dot()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var login = await client.GetStringAsync("/giris");
        var token = AntiforgeryTestHelper.GetToken(login);
        await client.PostAsync("/culture", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["culture"] = "en",
            ["returnUrl"] = "/giris",
            ["__RequestVerificationToken"] = token
        }));

        var email = $"en.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetStringAsync("/kayit");
        var registerToken = AntiforgeryTestHelper.GetToken(registerGet);
        var registerPost = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Ada Lovelace",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = registerToken
        }));

        registerPost.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await registerPost.Content.ReadAsStringAsync();
        wallet.Should().Contain("0.00 ₺");
        wallet.Should().Contain("Overview");
        wallet.Should().Contain("Out this month");
        wallet.Should().NotContain("0,00 ₺");
    }

    [Fact]
    public void FormatTry_follows_ui_culture_decimal_separator()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr");
            MoneyDisplay.FormatTry(0m).Should().Be("0,00 ₺");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            MoneyDisplay.FormatTry(0m).Should().Be("0.00 ₺");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de");
            MoneyDisplay.FormatTry(0m).Should().Be("0,00 ₺");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void Culture_cookie_value_uses_c_prefix()
    {
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en", "en"))
            .Should().StartWith("c=en");
    }
}
