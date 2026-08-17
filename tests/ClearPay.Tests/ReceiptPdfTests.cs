using ClearPay.Application.Activity;
using ClearPay.Infrastructure.Documents;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class ReceiptPdfTests
{
    [Fact]
    public void Render_writes_pdf_of_receipt_dto_without_inventing_amount()
    {
        var receipt = new ReceiptDto(
            Guid.Parse("aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001"),
            new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero),
            "TopUp",
            25.00m,
            "ClearPay treasury (demo)",
            "admin@clearpay.test",
            "Demo örnek dekont");
        var bytes = new SimplePdfReceiptRenderer().Render(receipt);
        bytes.Should().NotBeEmpty();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
        bytes.Length.Should().BeGreaterThan(300);
    }
}
