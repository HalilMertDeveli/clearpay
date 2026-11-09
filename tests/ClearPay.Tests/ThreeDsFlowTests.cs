using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClearPay.Tests;

/// <summary>
/// End-to-end over HTTP: merchant API → mock ACS page (OTP) → signed browser post → gateway callback →
/// redirect to the merchant, then capture / refund. Same host, SQLite ledger (see ClearPayWebFactory).
/// </summary>
public sealed class ThreeDsFlowTests : IClassFixture<ClearPayWebFactory>
{
    private const string MerchantKey = "dev-demo-shop-key";

    private readonly ClearPayWebFactory _factory;

    public ThreeDsFlowTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Merchant_api_needs_a_valid_key()
    {
        var client = _factory.CreateClient();
        var none = await client.PostAsJsonAsync("/api/payments", new { merchantOrderId = "x", amount = 1m });
        none.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var wrong = new HttpRequestMessage(HttpMethod.Get, $"/api/payments/{Guid.NewGuid()}");
        wrong.Headers.Add("X-Merchant-Key", "nope");
        (await client.SendAsync(wrong)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Correct_otp_authorizes_then_capture_and_partial_refunds_respect_the_limit()
    {
        var client = Browser();
        var (paymentId, redirectUrl) = await CreatePaymentAsync(client, 120.00m, "5555555555554444");
        redirectUrl.Should().Be($"/mock-acs/{paymentId}");

        var fields = await SubmitOtpAsync(client, redirectUrl, "123456");
        fields["result"].Should().Be("Y");

        var callback = await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(fields));
        callback.StatusCode.Should().Be(HttpStatusCode.Redirect);
        callback.Headers.Location!.ToString().Should()
            .StartWith("/demo-magaza/sonuc?paymentId=" + paymentId)
            .And.Contain("status=Authorized");

        // Browser back button / double submit: same redirect, still one authorization.
        var again = await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(fields));
        again.StatusCode.Should().Be(HttpStatusCode.Redirect);
        again.Headers.Location!.ToString().Should().Contain("status=Authorized");

        var payment = await GetPaymentAsync(client, paymentId);
        payment.GetProperty("status").GetString().Should().Be("Authorized");
        payment.GetProperty("provider").GetString().Should().Be("MockBankB");

        (await Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/capture")).StatusCode
            .Should().Be(HttpStatusCode.OK);
        (await Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/void")).StatusCode
            .Should().Be(HttpStatusCode.Conflict, "void is only allowed before capture");

        var partial = await Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/refunds", new { amount = 20.10m });
        partial.StatusCode.Should().Be(HttpStatusCode.Created);
        var over = await Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/refunds", new { amount = 99.91m });
        over.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var rest = await Merchant(client, HttpMethod.Post, $"/api/payments/{paymentId}/refunds", new { amount = 99.90m });
        rest.StatusCode.Should().Be(HttpStatusCode.Created);

        payment = await GetPaymentAsync(client, paymentId);
        payment.GetProperty("status").GetString().Should().Be("Refunded");
        payment.GetProperty("refundedAmount").GetDecimal().Should().Be(120.00m);

        var resultPage = await client.GetStringAsync(callback.Headers.Location!.ToString());
        resultPage.Should().Contain("Refunded");
    }

    [Fact]
    public async Task Wrong_otp_fails_the_payment_and_redirects_with_failed()
    {
        var client = Browser();
        var (paymentId, redirectUrl) = await CreatePaymentAsync(client, 15.00m, "4111111111111111");

        var fields = await SubmitOtpAsync(client, redirectUrl, "000000");
        fields["result"].Should().Be("N");
        var callback = await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(fields));

        callback.Headers.Location!.ToString().Should().Contain("status=Failed");
        (await GetPaymentAsync(client, paymentId)).GetProperty("failureCode").GetString().Should().Be("3DS_FAILED");
        (await client.GetStringAsync(redirectUrl)).Should().Contain("zaten tamamlandı");
    }

    [Fact]
    public async Task Tampered_callback_is_400_and_payment_stays_pending()
    {
        var client = Browser();
        var (paymentId, redirectUrl) = await CreatePaymentAsync(client, 15.00m, "4111111111111111");
        var fields = await SubmitOtpAsync(client, redirectUrl, "000000");

        fields["result"] = "Y"; // flip N → Y without re-signing
        var callback = await client.PostAsync("/api/payments/3ds/callback", new FormUrlEncodedContent(fields));

        callback.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetPaymentAsync(client, paymentId)).GetProperty("status").GetString().Should().Be("Pending3DS");
    }

    [Fact]
    public async Task Duplicate_merchant_order_is_409()
    {
        var client = Browser();
        var order = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        var body = new
        {
            merchantOrderId = order,
            amount = 10m,
            currency = "TRY",
            cardNumber = "4111111111111111",
            returnUrl = "/demo-magaza/sonuc"
        };
        (await Merchant(client, HttpMethod.Post, "/api/payments", body)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await Merchant(client, HttpMethod.Post, "/api/payments", body)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Demo_shop_page_starts_a_payment_and_redirects_to_the_acs()
    {
        var client = Browser();
        var html = await client.GetStringAsync("/demo-magaza");
        html.Should().Contain("MOCK / DEMO");

        var post = await client.PostAsync("/demo-magaza", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Amount"] = "49,90",
            ["CardNumber"] = "9792030000000018",
            ["__RequestVerificationToken"] = AntiforgeryTestHelper.GetToken(html)
        }));

        post.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var acs = post.Headers.Location!.ToString();
        acs.Should().StartWith("/mock-acs/");
        var acsHtml = await client.GetStringAsync(acs);
        acsHtml.Should().Contain("Mock PSP C").And.Contain("49,90");
    }

    private HttpClient Browser() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    internal static async Task<(Guid PaymentId, string RedirectUrl)> CreatePaymentAsync(
        HttpClient client,
        decimal amount,
        string card)
    {
        var response = await Merchant(client, HttpMethod.Post, "/api/payments", new
        {
            merchantOrderId = "ORD-" + Guid.NewGuid().ToString("N")[..10],
            amount,
            currency = "TRY",
            cardNumber = card,
            returnUrl = "/demo-magaza/sonuc"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("status").GetString().Should().Be("Pending3DS");
        return (doc.RootElement.GetProperty("paymentId").GetGuid(), doc.RootElement.GetProperty("redirectUrl").GetString()!);
    }

    /// <summary>GET the ACS page, post the OTP, and read the hidden fields the page would auto-submit.</summary>
    internal static async Task<Dictionary<string, string>> SubmitOtpAsync(HttpClient client, string acsUrl, string otp)
    {
        var page = await client.GetStringAsync(acsUrl);
        page.Should().Contain("123456", "the demo OTP hint is shown");
        var post = await client.PostAsync(acsUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Otp"] = otp,
            ["action"] = "confirm",
            ["__RequestVerificationToken"] = AntiforgeryTestHelper.GetToken(page)
        }));
        post.EnsureSuccessStatusCode();
        var html = await post.Content.ReadAsStringAsync();
        html.Should().Contain("action=\"/api/payments/3ds/callback\"");

        var fields = new Dictionary<string, string>();
        foreach (Match m in Regex.Matches(html, @"<input type=""hidden"" name=""(\w+)"" value=""([^""]*)"""))
        {
            if (m.Groups[1].Value != "__RequestVerificationToken")
                fields[m.Groups[1].Value] = m.Groups[2].Value;
        }

        fields.Keys.Should().BeEquivalentTo(new[] { "paymentId", "threeDsTransactionId", "result", "signature" });
        return fields;
    }

    private static async Task<JsonElement> GetPaymentAsync(HttpClient client, Guid paymentId)
    {
        var response = await Merchant(client, HttpMethod.Get, $"/api/payments/{paymentId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.Clone();
    }

    internal static Task<HttpResponseMessage> Merchant(HttpClient client, HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Merchant-Key", MerchantKey);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return client.SendAsync(request);
    }
}
