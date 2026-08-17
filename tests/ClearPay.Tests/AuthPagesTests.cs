using System.Net;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class AuthPagesTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public AuthPagesTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/giris")]
    [InlineData("/kayit")]
    [InlineData("/erisim-yok")]
    [InlineData("/api/health")]
    public async Task Anonymous_routes_return_200(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_page_has_email_password_and_register_link()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Account/Login");

        html.Should().Contain("E-posta");
        html.Should().Contain("Şifre");
        html.Should().Contain("Hesap oluştur");
        html.Should().Contain("Giriş");
        html.Should().Contain("Google ile giriş");
        html.Should().Contain("Apple ile giriş");
        html.Should().Contain("Beni hatırla");
    }

    [Fact]
    public async Task Access_denied_is_error_chrome_not_ninth_screen()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/erisim-yok");

        html.Should().Contain("Erişim yok");
        html.Should().Contain("empty-block");
        html.Should().Contain("Cüzdan özetine");
        html.Should().NotContain("Satıcı");
        html.Should().Contain("Demo — yükleme için sahte gateway");
    }

    [Fact]
    public async Task Register_page_has_name_email_and_password_fields()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Account/Register");

        html.Should().Contain("Ad");
        html.Should().Contain("E-posta");
        html.Should().Contain("Şifre tekrar");
        html.Should().Contain("Hesap oluştur");
        html.Should().Contain("Google ile giriş");
        html.Should().Contain("Apple ile giriş");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/hareketler")]
    public async Task Protected_routes_redirect_to_login(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/giris");
    }
}
