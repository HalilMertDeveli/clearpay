using System.Globalization;
using System.Text;
using ClearPay.Application.Activity;
using ClearPay.Application.Ports;

namespace ClearPay.Infrastructure.Documents;

/// <summary>
/// T-079: compact PDF 1.4 of an existing receipt. No native Skia; Helvetica + ASCII fold.
/// </summary>
public sealed class SimplePdfReceiptRenderer : IReceiptPdf
{
    public byte[] Render(ReceiptDto receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        var culture = CultureInfo.InvariantCulture;
        var amount = receipt.Amount.ToString("N2", culture) + " TRY";
        var when = receipt.At.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", culture);

        var lines = new List<string>
        {
            "ClearPay",
            "Dekont / Receipt",
            "",
            amount,
            "Status: Completed",
            "",
            "Date: " + when,
            "Kind: " + Fold(receipt.Kind),
            "Debit: " + Fold(receipt.DebitParty),
            "Credit: " + Fold(receipt.CreditParty)
        };
        if (!string.IsNullOrWhiteSpace(receipt.Description))
            lines.Add("Note: " + Fold(receipt.Description));
        if (!string.IsNullOrWhiteSpace(receipt.InstrumentHint))
            lines.Add("Instrument: " + Fold(receipt.InstrumentHint));
        lines.Add("Correlation: " + receipt.CorrelationId.ToString("D"));
        lines.Add("");
        lines.Add("Demo -- fake gateway for top-ups");
        return SimplePdf.FromLines(lines);
    }

    private static string Fold(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        var folded = value
            .Replace('ş', 's').Replace('Ş', 'S')
            .Replace('ı', 'i').Replace('İ', 'I')
            .Replace('ğ', 'g').Replace('Ğ', 'G')
            .Replace('ü', 'u').Replace('Ü', 'U')
            .Replace('ö', 'o').Replace('Ö', 'O')
            .Replace('ç', 'c').Replace('Ç', 'C')
            .Replace('₺', ' ');
        var buffer = new StringBuilder(folded.Length);
        foreach (var ch in folded)
            buffer.Append(ch <= 127 ? ch : '?');
        return buffer.ToString();
    }
}

internal static class SimplePdf
{
    public static byte[] FromLines(IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var content = new StringBuilder();
        content.Append("BT\n/F1 18 Tf\n50 800 Td\n14 TL\n");
        var first = true;
        foreach (var line in lines)
        {
            if (!first)
                content.Append("T*\n");
            first = false;
            var font = line is "ClearPay" or "Dekont / Receipt" ? "18" : "11";
            if (line == "ClearPay")
                content.Append("/F1 18 Tf\n");
            else if (line == "Dekont / Receipt")
                content.Append("/F1 12 Tf\n");
            else if (font == "11")
                content.Append("/F1 11 Tf\n");
            content.Append('(').Append(Escape(line)).Append(") Tj\n");
        }

        content.Append("ET\n");
        var stream = content.ToString();
        var streamBytes = Encoding.ASCII.GetBytes(stream);

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            "<< /Length " + streamBytes.Length + " >>\nstream\n" + stream + "endstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        using var output = new MemoryStream();
        void Write(string text) => output.Write(Encoding.ASCII.GetBytes(text));

        Write("%PDF-1.4\n");
        var offsets = new long[objects.Length + 1];
        for (var i = 0; i < objects.Length; i++)
        {
            offsets[i + 1] = output.Position;
            Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xref = output.Position;
        Write($"xref\n0 {objects.Length + 1}\n");
        Write("0000000000 65535 f \n");
        for (var i = 1; i <= objects.Length; i++)
            Write($"{offsets[i]:0000000000} 00000 n \n");
        Write($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return output.ToArray();
    }

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
