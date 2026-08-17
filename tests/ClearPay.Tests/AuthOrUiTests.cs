using System.Net;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Identity;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        body.Should().Contain("\"redis\":\"off\"");
        body.Should().Contain("\"rabbit\":\"off\"");
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
        wallet.Should().Contain("masthead");
        wallet.Should().Contain("Hızlı işlemler");
        wallet.Should().Contain("İnternet");
        wallet.Should().NotContain("Worldcard");
        wallet.Should().NotContain(">Admin<");
    }

    [Fact]
    public async Task YukleCek_shows_demo_card_panel_without_ninth_screen()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var email = $"kart.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetAsync("/Account/Register");
        var token = AuthFormToken.Get(await registerGet.Content.ReadAsStringAsync());
        await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Kart Deneme",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        var html = await client.GetStringAsync("/yukle-cek");
        html.Should().Contain("id=\"kart\"");
        html.Should().Contain("demo-card");
        html.Should().Contain("Son 4 hane");
        html.Should().Contain("Kart ekle");
        html.Should().Contain("Henüz kart yok");
        html.Should().NotContain("asp-for=\"NewCard.Pan\"");
        html.Should().NotContain("name=\"NewCard.Cvv\"");
        html.Should().NotContain(">Kartlar<");
        html.Should().Contain("Yükle / Çek");
        html.Should().Contain("Havale");
        html.Should().Contain("Hareketler");
    }

    [Fact]
    public async Task Havale_send_is_disabled_when_wallet_is_empty()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var email = $"bos.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetAsync("/Account/Register");
        var token = AuthFormToken.Get(await registerGet.Content.ReadAsStringAsync());
        await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Boş Cüzdan",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        var html = await client.GetStringAsync("/havale");
        html.Should().Contain("Kalan bakiye");
        html.Should().Contain("disabled");
        html.Should().Contain("Gönder");
        html.Should().Contain("Aynı işlem iki kez kesilmez");
        html.Should().Contain("handler=Review");
        html.Should().NotContain("Onayla ve gönder");
    }

    [Fact]
    public async Task Hareketler_has_to_date_filter_and_empty_cta()
    {
        var client = CreateClient();
        if (!await LoginPageExistsAsync(client))
        {
            return;
        }

        var email = $"tar.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetAsync("/Account/Register");
        var token = AuthFormToken.Get(await registerGet.Content.ReadAsStringAsync());
        await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Tarih Filtre",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        var html = await client.GetStringAsync("/hareketler");
        html.Should().Contain("name=\"Bitis\"");
        html.Should().Contain("Bitiş");
        html.Should().Contain("Havale gönder");
        html.Should().Contain("skip-link");
        html.Should().NotContain(">Kartlar<");
    }

    [Fact]
    public async Task Dekont_page_offers_pdf_of_existing_ledger_row()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });
        if (!await LoginPageExistsAsync(client))
            return;

        var email = $"pdf.{Guid.NewGuid():N}@clearpay.test";
        var registerGet = await client.GetAsync("/Account/Register");
        var token = AuthFormToken.Get(await registerGet.Content.ReadAsStringAsync());
        await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = "Pdf Deneme",
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = token
        }));

        Guid correlation;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userId = (await users.FindByEmailAsync(email))!.Id;
            var db = scope.ServiceProvider.GetRequiredService<ClearPayDbContext>();
            var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet is null)
            {
                wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTimeOffset.UtcNow };
                db.Wallets.Add(wallet);
            }

            var treasury = new Wallet { Id = Guid.NewGuid(), UserId = "fund-pdf-" + Guid.NewGuid().ToString("N"), CreatedAt = DateTimeOffset.UtcNow };
            db.Wallets.Add(treasury);
            correlation = Guid.NewGuid();
            var (debit, credit) = LedgerPair.Create(treasury.Id, wallet.Id, 25m, correlation, LedgerEntryKind.TopUp);
            db.LedgerEntries.AddRange(debit, credit);
            await db.SaveChangesAsync();
        }

        var html = await client.GetStringAsync($"/dekont/{correlation}");
        html.Should().Contain("PDF indir");
        html.Should().Contain("handler=Pdf");
        html.Should().NotContain("Worldcard");

        var pdf = await client.GetAsync($"/dekont/{correlation}?handler=Pdf");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
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
