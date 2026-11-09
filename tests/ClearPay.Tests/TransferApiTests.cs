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

public sealed class TransferApiTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public TransferApiTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Duplicate_transfer_returns_409()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"gonderen.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"alici.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Gönderen");
        await RegisterAsync(client, recipientEmail, "Alıcı");

        string senderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sender = await users.FindByEmailAsync(senderEmail);
            sender.Should().NotBeNull();
            senderId = sender!.Id;
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), senderId, 80m);
        }

        var token = await GetTokenAsync(client, senderEmail);
        var key = "idem-" + Guid.NewGuid().ToString("N");
        var first = await PostTransferAsync(client, token, key, recipientEmail, 25m);
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var second = await PostTransferAsync(client, token, key, recipientEmail, 25m);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var verify = _factory.Services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        var wallet = await db.Wallets.SingleAsync(w => w.UserId == senderId);
        var rows = await db.LedgerEntries.AsNoTracking().Where(e => e.WalletId == wallet.Id).ToListAsync();
        LedgerPair.NetOf(rows, wallet.Id).Should().Be(55m);
        (await db.Transfers.CountAsync(t => t.FromWalletId == wallet.Id)).Should().Be(1);
        var all = await db.LedgerEntries.AsNoTracking().ToListAsync();
        all.Sum(e => e.Amount).Should().Be(0m, "double-entry: every debit has a credit");
    }

    [Fact]
    public async Task Frozen_wallet_returns_403_and_does_not_move_money()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"frozen.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"hedef.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Dondurulmuş");
        await RegisterAsync(client, recipientEmail, "Hedef");

        string senderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sender = await users.FindByEmailAsync(senderEmail);
            sender.Should().NotBeNull();
            senderId = sender!.Id;
            var db = scope.ServiceProvider.GetRequiredService<ClearPayDbContext>();
            await FundAsync(db, senderId, 80m);
            var wallet = await db.Wallets.SingleAsync(w => w.UserId == senderId);
            wallet.IsFrozen = true;
            await db.SaveChangesAsync();
        }

        var token = await GetTokenAsync(client, senderEmail);
        var response = await PostTransferAsync(client, token, "frz-" + Guid.NewGuid().ToString("N"), recipientEmail, 10m);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var verify = _factory.Services.CreateScope();
        var ledger = verify.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        var walletId = (await ledger.Wallets.SingleAsync(w => w.UserId == senderId)).Id;
        (await ledger.Transfers.CountAsync(t => t.FromWalletId == walletId)).Should().Be(0);
        var rows = await ledger.LedgerEntries.AsNoTracking().Where(e => e.WalletId == walletId).ToListAsync();
        LedgerPair.NetOf(rows, walletId).Should().Be(80m);
    }

    [Fact]
    public async Task Missing_idempotency_key_returns_400()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"keyless.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"karsi.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Keysiz");
        await RegisterAsync(client, recipientEmail, "Karşı");

        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sender = await users.FindByEmailAsync(senderEmail);
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), sender!.Id, 20m);
        }

        var token = await GetTokenAsync(client, senderEmail);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transfers")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { recipient = recipientEmail, amount = 1m, description = "demo" }),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Same_key_different_amount_returns_409_without_second_debit()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"hash.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"alici2.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Hash");
        await RegisterAsync(client, recipientEmail, "Alıcı2");

        string senderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sender = await users.FindByEmailAsync(senderEmail);
            senderId = sender!.Id;
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), senderId, 80m);
        }

        var token = await GetTokenAsync(client, senderEmail);
        var key = "mix-" + Guid.NewGuid().ToString("N");
        (await PostTransferAsync(client, token, key, recipientEmail, 25m)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostTransferAsync(client, token, key, recipientEmail, 40m)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var verify = _factory.Services.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        var wallet = await db.Wallets.SingleAsync(w => w.UserId == senderId);
        var rows = await db.LedgerEntries.AsNoTracking().Where(e => e.WalletId == wallet.Id).ToListAsync();
        LedgerPair.NetOf(rows, wallet.Id).Should().Be(55m);
        (await db.Transfers.CountAsync(t => t.FromWalletId == wallet.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Transfer_without_jwt_is_401_and_missing_money_is_422()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"bos.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"diger.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Boş");
        await RegisterAsync(client, recipientEmail, "Diğer");

        var anonymous = await PostTransferAsync(client, token: null, "k", recipientEmail, 1m);
        anonymous.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var token = await GetTokenAsync(client, senderEmail);
        var poor = await PostTransferAsync(client, token, "poor-" + Guid.NewGuid().ToString("N"), recipientEmail, 1m);
        poor.StatusCode.Should().Be((HttpStatusCode)422);
    }

    [Fact]
    public async Task Get_transfer_by_id_returns_201_location_and_hides_strangers()
    {
        var client = _factory.CreateClient();
        var senderEmail = $"get.{Guid.NewGuid():N}@clearpay.test";
        var recipientEmail = $"hedef.{Guid.NewGuid():N}@clearpay.test";
        var strangerEmail = $"yabanci.{Guid.NewGuid():N}@clearpay.test";
        await RegisterAsync(client, senderEmail, "Gönderen");
        await RegisterAsync(client, recipientEmail, "Alıcı");
        await RegisterAsync(client, strangerEmail, "Yabancı");

        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sender = await users.FindByEmailAsync(senderEmail);
            await FundAsync(scope.ServiceProvider.GetRequiredService<ClearPayDbContext>(), sender!.Id, 80m);
        }

        var token = await GetTokenAsync(client, senderEmail);
        var created = await PostTransferAsync(client, token, "get-" + Guid.NewGuid().ToString("N"), recipientEmail, 12m);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var transferId = body.RootElement.GetProperty("transferId").GetGuid();
        created.Headers.Location.Should().NotBeNull();
        created.Headers.Location!.ToString().Should().Contain(transferId.ToString());

        using var got = await GetJsonAsync(client, token, $"/api/transfers/{transferId}");
        got.RootElement.GetProperty("transferId").GetGuid().Should().Be(transferId);
        got.RootElement.GetProperty("amount").GetDecimal().Should().Be(12m);

        var strangerToken = await GetTokenAsync(client, strangerEmail);
        (await GetRawAsync(client, strangerToken, $"/api/transfers/{transferId}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

        var anon = await client.GetAsync($"/api/transfers/{transferId}");
        anon.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        anon.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problem = await anon.Content.ReadAsStringAsync();
        problem.Should().Contain("Unauthorized");
    }

    private static async Task RegisterAsync(HttpClient client, string email, string name)
    {
        var page = await client.GetStringAsync("/Account/Register");
        var antiforgery = AntiforgeryTestHelper.GetToken(page);
        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(
            RegisterForm.Cookie(antiforgery, email, name)));
        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/token", new { email, password = "Deneme123" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    private static async Task<HttpResponseMessage> PostTransferAsync(
        HttpClient client,
        string? token,
        string key,
        string recipient,
        decimal amount)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transfers")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { recipient, amount, description = "demo" }),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static async Task<JsonDocument> GetJsonAsync(HttpClient client, string token, string url)
    {
        using var response = await GetRawAsync(client, token, url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static async Task<HttpResponseMessage> GetRawAsync(HttpClient client, string token, string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
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
