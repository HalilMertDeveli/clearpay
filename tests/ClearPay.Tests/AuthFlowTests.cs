using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClearPay.Tests;

public sealed class AuthFlowTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public AuthFlowTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Giris_and_kayit_pages_show_forms()
    {
        var client = _factory.CreateClient();
        var login = await client.GetStringAsync("/giris");
        login.Should().Contain("E-posta");
        login.Should().Contain("Şifre");
        login.Should().Contain("Hesap oluştur");
        login.Should().Contain("ClearPay (Demo)");

        var register = await client.GetStringAsync("/kayit");
        register.Should().Contain("Ad");
        register.Should().Contain("Şifre tekrar");
        register.Should().Contain("Hesap oluştur");
    }

    [Fact]
    public async Task Register_signs_in_and_shows_empty_wallet()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });
        var email = $"ayse.{Guid.NewGuid():N}@clearpay.test";

        var html = await client.GetStringAsync("/kayit");
        var token = GetToken(html);
        var post = await client.PostAsync("/kayit", new FormUrlEncodedContent(
            RegisterForm.Cookie(token, email, "Ayşe Yılmaz")));

        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var wallet = await post.Content.ReadAsStringAsync();
        wallet.Should().Contain("0,00 ₺");
        wallet.Should().Contain("Henüz hareket yok.");
        wallet.Should().Contain("Havale gönder");
        wallet.Should().NotContain(">Admin<");
    }

    private static string GetToken(string html)
    {
        var match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(html, @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""", RegexOptions.IgnoreCase);
        }

        match.Success.Should().BeTrue("antiforgery token expected");
        return match.Groups[1].Value;
    }
}
