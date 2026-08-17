using System.Net;
using ClearPay.Application.Identity;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
        html.Should().Contain("TC (demo)");
        html.Should().Contain("10000000146");
        html.Should().NotContain("Şifremi unuttum");
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
        html.Should().Contain("Telefon");
        html.Should().Contain("Bireysel");
        html.Should().Contain("Kurumsal");
        html.Should().Contain("Şifre tekrar");
        html.Should().Contain("Hesap oluştur");
        html.Should().Contain("Google ile giriş");
        html.Should().Contain("Apple ile giriş");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/kartlar")]
    [InlineData("/hareketler")]
    public async Task Protected_routes_redirect_to_login(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/giris");
    }

    [Fact]
    public async Task Register_stores_phone_and_account_kind()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var email = $"tel.{Guid.NewGuid():N}@clearpay.test";
        var phone = RegisterForm.UniquePhone();
        var html = await client.GetStringAsync("/Account/Register");
        var post = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(
            RegisterForm.Cookie(AntiforgeryTestHelper.GetToken(html), email, "Tel Deneme", phone, AccountKinds.Kurumsal)));

        post.EnsureSuccessStatusCode();
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        user.Should().NotBeNull();
        user!.PhoneNumber.Should().Be(TurkishPhone.Normalize(phone));
        user.AccountKind.Should().Be(AccountKinds.Kurumsal);
    }

    [Fact]
    public async Task Login_with_demo_tc_signs_in_seed_admin()
    {
        var client = _factory.CreateClient(new() { HandleCookies = true });
        var loginHtml = await client.GetStringAsync("/Account/Login");
        var post = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Tc"] = DemoTc.AdminNationalId,
            ["Input.Password"] = "Deneme123",
            ["__RequestVerificationToken"] = AntiforgeryTestHelper.GetToken(loginHtml)
        }));

        post.EnsureSuccessStatusCode();
        var wallet = await post.Content.ReadAsStringAsync();
        wallet.Should().Contain("Özet");
        wallet.Should().Contain("masthead");
        wallet.Should().NotContain("name=\"Input.Tc\"");
    }
}
