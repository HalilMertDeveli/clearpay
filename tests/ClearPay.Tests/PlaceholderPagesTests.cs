using System.Net;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class PlaceholderPagesTests : IClassFixture<ClearPayWebFactory>
{
    private readonly HttpClient _client;

    public PlaceholderPagesTests(ClearPayWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/havale")]
    [InlineData("/yukle-cek")]
    [InlineData("/hareketler")]
    [InlineData("/api/health")]
    public async Task Placeholder_routes_return_200(string path)
    {
        var response = await _client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
