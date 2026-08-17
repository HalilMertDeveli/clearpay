using System.Net;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class SwaggerTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public SwaggerTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenApi_documents_transfer_409_and_idempotency_key()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Idempotency-Key");
        json.Should().Contain("409");
        json.Should().Contain("/api/transfers");
        json.Should().Contain("/api/wallet");
        json.Should().Contain("/api/movements");
        json.Should().Contain("/api/receipts");
        json.Should().Contain("/pdf");
        json.Should().Contain("/api/topup");
        json.Should().Contain("/api/withdraw");
        json.Should().Contain("wallet was not charged");
    }
}
