using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClearPay.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Tests;

public sealed class PasswordRecoveryTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public PasswordRecoveryTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Forgot_does_not_reveal_whether_email_or_phone_exists()
    {
        var client = _factory.CreateClient();
        var unknownEmail = await client.PostAsJsonAsync("/api/password/forgot", new
        {
            email = $"yok.{Guid.NewGuid():N}@clearpay.test"
        });
        var unknownPhone = await client.PostAsJsonAsync("/api/password/forgot", new { phone = "5559990000" });
        var adminEmail = await client.PostAsJsonAsync("/api/password/forgot", new
        {
            email = IdentitySeeder.DevelopmentAdminEmail
        });
        var adminPhone = await client.PostAsJsonAsync("/api/password/forgot", new
        {
            phone = IdentitySeeder.DevelopmentAdminPhoneLocal
        });

        unknownEmail.StatusCode.Should().Be(HttpStatusCode.OK);
        unknownPhone.StatusCode.Should().Be(HttpStatusCode.OK);
        adminEmail.StatusCode.Should().Be(HttpStatusCode.OK);
        adminPhone.StatusCode.Should().Be(HttpStatusCode.OK);

        var a = await unknownEmail.Content.ReadAsStringAsync();
        (await unknownPhone.Content.ReadAsStringAsync()).Should().Be(a);
        (await adminEmail.Content.ReadAsStringAsync()).Should().Be(a);
        (await adminPhone.Content.ReadAsStringAsync()).Should().Be(a);
        a.Should().Contain("Varsa e-posta kuyruğa alındı");
        a.Should().NotContain("token");
        a.Should().NotContain("Deneme");
    }

    [Fact]
    public async Task Reset_with_bad_token_fails()
    {
        var client = _factory.CreateClient();
        var email = $"reset.{Guid.NewGuid():N}@clearpay.test";
        (await client.PostAsJsonAsync("/api/register", new
        {
            fullName = "Reset",
            email,
            password = "Deneme123",
            confirmPassword = "Deneme123",
            phone = UniquePhone()
        })).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/password/reset", new
        {
            email,
            token = "not-a-real-token",
            newPassword = "Deneme456"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reset_with_usermanager_token_works_and_seed_admin_phone_is_lookupable()
    {
        var client = _factory.CreateClient();
        var email = $"ok.{Guid.NewGuid():N}@clearpay.test";
        (await client.PostAsJsonAsync("/api/register", new
        {
            fullName = "Ok",
            email,
            password = "Deneme123",
            confirmPassword = "Deneme123",
            phone = UniquePhone()
        })).EnsureSuccessStatusCode();

        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            user.Should().NotBeNull();
            user!.PhoneNumber.Should().NotBeNullOrWhiteSpace();
            user.PhoneNumber.Should().StartWith("90");
            token = await users.GeneratePasswordResetTokenAsync(user);

            var admin = await users.FindByEmailAsync(IdentitySeeder.DevelopmentAdminEmail);
            admin.Should().NotBeNull();
            admin!.PhoneNumber.Should().Be(IdentitySeeder.DevelopmentAdminPhone);
        }

        var reset = await client.PostAsJsonAsync("/api/password/reset", new
        {
            email,
            token,
            newPassword = "Deneme456"
        });
        reset.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await client.PostAsJsonAsync("/api/token", new { email, password = "Deneme456" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Firebase_test_token_provisions_sql_user_with_phone()
    {
        var client = _factory.CreateClient();
        var email = $"fb.{Guid.NewGuid():N}@clearpay.test";
        var uid = Guid.NewGuid().ToString("N");
        var idToken = FirebaseIdTokenVerifier.CreateTestToken(uid, email);

        var created = await client.PostAsJsonAsync("/api/token/firebase", new
        {
            idToken,
            fullName = "Firebase User",
            phone = UniquePhone(),
            accountKind = "Bireysel"
        });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        using var first = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        first.RootElement.GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();

        var again = await client.PostAsJsonAsync("/api/token/firebase", new { idToken });
        again.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            user.Should().NotBeNull();
            user!.PhoneNumber.Should().NotBeNullOrWhiteSpace();
            user.PhoneNumber.Should().StartWith("90");
            (await users.FindByLoginAsync(FirebaseIdTokenVerifier.LoginProvider, uid)).Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Firebase_bad_token_is_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/token/firebase", new
        {
            idToken = "not-valid",
            fullName = "X",
            phone = UniquePhone()
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string UniquePhone()
    {
        var n = Random.Shared.Next(100_000_000, 999_999_999);
        return "5" + n;
    }
}
