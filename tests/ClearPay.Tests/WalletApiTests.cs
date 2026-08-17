using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Identity;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Tests;

public sealed class WalletApiTests : IClassFixture<ClearPayWebFactory>
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly ClearPayWebFactory _factory;

    public WalletApiTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Wallet_without_jwt_is_401_with_jwt_matches_ledger_net()
    {
        var client = _factory.CreateClient();
        (await client.GetAsync("/api/wallet")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var email = $"ozet.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, email, "Özet");
        string userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            userId = (await users.FindByEmailAsync(email))!.Id;
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), userId, 80m);
        }

        var token = await GetTokenAsync(client, email);
        using var wallet = await GetJsonAsync(client, token, "/api/wallet");
        wallet.RootElement.GetProperty("balance").GetDecimal().Should().Be(80m);
        wallet.RootElement.GetProperty("isFrozen").GetBoolean().Should().BeFalse();
        wallet.RootElement.GetProperty("userId").GetString().Should().Be(userId);
    }

    [Fact]
    public async Task Transfer_then_wallet_get_shows_same_net()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"sen.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"recv.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Gönderen");
        await RegisterAsync(client, recipientEmail, "Alıcı");

        string senderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            senderId = (await users.FindByEmailAsync(senderEmail))!.Id;
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), senderId, 80m);
        }

        var token = await GetTokenAsync(client, senderEmail);
        var transfer = await PostMoneyAsync(
            client, token, "/api/transfers", "idem-" + Guid.NewGuid().ToString("N"),
            new { recipient = recipientEmail, amount = 25m, description = "demo" });
        transfer.StatusCode.Should().Be(HttpStatusCode.Created);

        using var wallet = await GetJsonAsync(client, token, "/api/wallet");
        wallet.RootElement.GetProperty("balance").GetDecimal().Should().Be(55m);

        using var movements = await GetJsonAsync(client, token, "/api/movements");
        movements.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        var first = movements.RootElement.GetProperty("items")[0];
        var corr = first.GetProperty("correlationId").GetGuid();

        using var receipt = await GetJsonAsync(client, token, $"/api/receipts/{corr}");
        receipt.RootElement.GetProperty("correlationId").GetGuid().Should().Be(corr);
        receipt.RootElement.GetProperty("amount").GetDecimal().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Duplicate_topup_returns_409_and_does_not_credit_twice()
    {
        var client = _factory.CreateClient();
        var email = $"yukle.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, email, "Yükle");
        string userId;
        using (var scope = _factory.Services.CreateScope())
        {
            userId = (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
                .FindByEmailAsync(email))!.Id;
        }

        var token = await GetTokenAsync(client, email);
        var key = "top-" + Guid.NewGuid().ToString("N");
        var body = new { amount = 40m, account = "****1234" };
        (await PostMoneyAsync(client, token, "/api/topup", key, body)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostMoneyAsync(client, token, "/api/topup", key, body)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var verify = _factory.Services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        var wallet = await db.Wallets.SingleAsync(w => w.UserId == userId);
        var rows = await db.LedgerEntries.AsNoTracking().Where(e => e.WalletId == wallet.Id).ToListAsync();
        LedgerPair.NetOf(rows, wallet.Id).Should().Be(40m);
    }

    [Fact]
    public async Task Cors_allows_localhost_origin_on_wallet()
    {
        var client = _factory.CreateClient();
        var email = $"cors.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, email, "Cors");
        var token = await GetTokenAsync(client, email);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("Origin", "http://localhost:8080");
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:8080");
    }

    [Fact]
    public async Task Register_api_returns_jwt_and_wallet_then_cards()
    {
        var client = _factory.CreateClient();
        var email = $"app.{Guid.NewGuid():N}@clearpay.test";
        var created = await client.PostAsJsonAsync("/api/register", new
        {
            fullName = "Mobil Kayıt",
            email,
            password = "Deneme123",
            confirmPassword = "Deneme123"
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("access_token").GetString();
        token.Should().NotBeNullOrWhiteSpace();

        using var wallet = await GetJsonAsync(client, token!, "/api/wallet");
        wallet.RootElement.GetProperty("balance").GetDecimal().Should().Be(0m);

        var add = await PostJsonAsync(client, token!, "/api/cards", new { last4 = "4242", label = "Demo" });
        add.StatusCode.Should().Be(HttpStatusCode.Created);
        using var list = await GetJsonAsync(client, token!, "/api/cards");
        list.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Customer_cannot_call_admin_freeze()
    {
        var client = _factory.CreateClient();
        var email = $"musteri.{Guid.NewGuid():N}@clearpay.test";
        var created = await client.PostAsJsonAsync("/api/register", new
        {
            fullName = "Musteri",
            email,
            password = "Deneme123",
            confirmPassword = "Deneme123"
        });
        created.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("access_token").GetString()!;
        var freeze = await PostJsonAsync(client, token, "/api/admin/freeze", new { email });
        freeze.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Receipt_for_unknown_id_is_404()
    {
        var client = _factory.CreateClient();
        var email = $"dekont.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, email, "Dekont");
        var token = await GetTokenAsync(client, email);
        var response = await SendAsync(client, token, HttpMethod.Get, $"/api/receipts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task RegisterAsync(HttpClient client, string email, string name)
    {
        var page = await client.GetStringAsync("/Account/Register");
        var antiforgery = AntiforgeryTestHelper.GetToken(page);
        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.FullName"] = name,
            ["Input.Email"] = email,
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["__RequestVerificationToken"] = antiforgery
        }));
        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/token", new { email, password = "Deneme123" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    private static async Task<JsonDocument> GetJsonAsync(HttpClient client, string token, string url)
    {
        var response = await SendAsync(client, token, HttpMethod.Get, url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        string token,
        HttpMethod method,
        string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostMoneyAsync(
        HttpClient client,
        string token,
        string url,
        string key,
        object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostJsonAsync(
        HttpClient client,
        string token,
        string url,
        object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static async Task FundAsync(ClearPayDbContext db, string userId, decimal amount)
    {
        var sender = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (sender is null)
        {
            sender = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Wallets.Add(sender);
        }

        var funder = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = "fund-" + Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Wallets.Add(funder);
        var (debit, credit) = LedgerPair.Create(
            funder.Id, sender.Id, amount, Guid.NewGuid(), LedgerEntryKind.TopUp);
        db.LedgerEntries.AddRange(debit, credit);
        await db.SaveChangesAsync();
    }
}
