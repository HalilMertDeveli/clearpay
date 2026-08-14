using System.Net;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class PlaceholderPagesTests : IClassFixture<ClearPayWebFactory>
{
    private readonly HttpClient _client;

    public PlaceholderPagesTests(ClearPayWebFactory factory)
    {
        _client = factory.CreateClient(new() { AllowAutoRedirect = false });
    }

    [Theory]
    [InlineData("/giris")]
    [InlineData("/kayit")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/api/health")]
    public async Task Anonymous_routes_return_200(string path)
    {
        var response = await _client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/hareketler")]
    [InlineData("/admin")]
    [InlineData("/dekont/00000000-0000-0000-0000-000000000000")]
    public async Task Wallet_routes_redirect_to_login(string path)
    {
        var response = await _client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/giris");
    }
}
