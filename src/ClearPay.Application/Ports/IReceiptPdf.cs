using ClearPay.Application.Activity;

namespace ClearPay.Application.Ports;

/// <summary>
/// T-079: PDF of an existing <see cref="ReceiptDto"/>. Not a second ledger.
/// </summary>
public interface IReceiptPdf
{
    byte[] Render(ReceiptDto receipt);
}
