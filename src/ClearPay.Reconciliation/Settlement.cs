using System.Globalization;
using System.Text;

namespace ClearPay.Reconciliation;

/// <summary>One line of an acquirer's end-of-day settlement file (or ClearPay's own view of the same money).</summary>
public sealed record SettlementLine(string Provider, string Kind, string Reference, decimal Amount, string Currency);

/// <summary>
/// The settlement file format used by all three mock acquirers:
/// <code>
/// # provider=MockBankA;date=2026-11-09;source=clearpay-mock-generator
/// record_type,reference,amount,currency
/// SALE,8d6f...,120.00,TRY
/// REFUND,1c2e...,20.10,TRY
/// </code>
/// Real banks each have their own layout (fixed width, ISO 20022 camt.053, ...); one parser is enough here.
/// A broken line throws <see cref="FormatException"/> with its line number: a half-read file must fail the
/// job, not produce a report that looks clean.
/// </summary>
public static class SettlementCsv
{
    public const string Header = "record_type,reference,amount,currency";
    public const string GeneratedMarker = "source=clearpay-mock-generator";

    public static string Write(string provider, DateOnly day, IEnumerable<SettlementLine> lines)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"# provider={provider};date={day:yyyy-MM-dd};{GeneratedMarker}\n");
        sb.Append(Header).Append('\n');
        foreach (var line in lines)
        {
            sb.Append(CultureInfo.InvariantCulture,
                $"{line.Kind},{line.Reference},{line.Amount.ToString("F2", CultureInfo.InvariantCulture)},{line.Currency}\n");
        }

        return sb.ToString();
    }

    public static IReadOnlyList<SettlementLine> Parse(string provider, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var result = new List<SettlementLine>();
        var lineNumber = 0;
        foreach (var raw in content.Split('\n'))
        {
            lineNumber++;
            var text = raw.TrimEnd('\r').Trim();
            if (text.Length == 0 || text.StartsWith('#') || text == Header)
                continue;

            var parts = text.Split(',');
            if (parts.Length != 4)
                throw new FormatException($"{provider} line {lineNumber}: expected 4 fields, got {parts.Length}.");
            var kind = parts[0].Trim().ToUpperInvariant();
            if (kind is not ("SALE" or "REFUND"))
                throw new FormatException($"{provider} line {lineNumber}: unknown record type '{parts[0]}'.");
            if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                throw new FormatException($"{provider} line {lineNumber}: amount '{parts[2]}' is not a positive number.");
            var currency = parts[3].Trim().ToUpperInvariant();
            if (currency.Length != 3)
                throw new FormatException($"{provider} line {lineNumber}: currency '{parts[3]}' is not a 3-letter code.");
            var reference = parts[1].Trim();
            if (reference.Length == 0)
                throw new FormatException($"{provider} line {lineNumber}: empty reference.");

            result.Add(new SettlementLine(provider, kind, reference, amount, currency));
        }

        return result;
    }
}

/// <summary>
/// MOCK. The mock acquirers in the web app are stateless (they answer from the amount's kuruş digits and
/// keep no ledger), so there is no real bank-side record to export. This generator plays the bank: it
/// starts from the money ClearPay saw for that provider and day and applies fixed, documented anomalies,
/// so every reconciliation outcome can be produced on purpose:
/// <list type="bullet">
/// <item>SALE ending in <b>.37</b>: the bank settled 1.00 more → <c>AmountMismatch</c></item>
/// <item>SALE ending in <b>.73</b>: the bank file does not have it → <c>OnlyInClearPay</c></item>
/// <item>SALE ending in <b>.66</b>: the bank also sends a line ClearPay never saw → <c>OnlyAtBank</c></item>
/// </list>
/// Everything else matches. A file that someone dropped into the folder (no generator marker) is used as is.
/// </summary>
public static class MockSettlementGenerator
{
    public static IReadOnlyList<SettlementLine> BankView(IEnumerable<SettlementLine> ours)
    {
        var bank = new List<SettlementLine>();
        foreach (var line in ours)
        {
            var kurus = Kurus(line.Amount);
            if (line.Kind == "SALE" && kurus == 73)
                continue;
            if (line.Kind == "SALE" && kurus == 37)
            {
                bank.Add(line with { Amount = line.Amount + 1.00m });
                continue;
            }

            bank.Add(line);
            if (line.Kind == "SALE" && kurus == 66)
                bank.Add(line with { Reference = "BANK-" + line.Reference[..Math.Min(8, line.Reference.Length)].ToUpperInvariant() });
        }

        return bank;
    }

    private static int Kurus(decimal amount) => (int)(decimal.Round(amount * 100m) % 100m);
}

/// <summary>Where settlement files come from. Production: an SFTP pull. Here: a folder plus the mock generator.</summary>
public interface ISettlementFileSource
{
    /// <summary>Returns the provider's file for the day. <paramref name="ours"/> is only used by the mock generator.</summary>
    Task<IReadOnlyList<SettlementLine>> GetAsync(
        string provider,
        DateOnly day,
        IReadOnlyList<SettlementLine> ours,
        CancellationToken cancellationToken);
}

/// <summary>
/// <c>{SettlementDirectory}/{provider}/{yyyy-MM-dd}.csv</c>. A file without the generator marker was "received"
/// and is read as is; otherwise the mock bank (re)generates it, then it is parsed like any received file.
/// </summary>
public sealed class FolderSettlementFileSource : ISettlementFileSource
{
    private readonly string _root;

    public FolderSettlementFileSource(string root)
    {
        _root = root;
    }

    public string PathFor(string provider, DateOnly day) =>
        Path.Combine(_root, provider, day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".csv");

    public async Task<IReadOnlyList<SettlementLine>> GetAsync(
        string provider,
        DateOnly day,
        IReadOnlyList<SettlementLine> ours,
        CancellationToken cancellationToken)
    {
        var path = PathFor(provider, day);
        if (File.Exists(path))
        {
            var existing = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            if (!existing.Contains(SettlementCsv.GeneratedMarker, StringComparison.Ordinal))
                return SettlementCsv.Parse(provider, existing);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var content = SettlementCsv.Write(provider, day, MockSettlementGenerator.BankView(ours));
        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
        return SettlementCsv.Parse(provider, content);
    }
}
