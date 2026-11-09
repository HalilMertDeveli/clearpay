using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace ClearPay.Web.Observability;

/// <summary>
/// Last line of defence: even if someone logs a card number or password by mistake, it does not
/// reach Seq. (1) Properties with a sensitive name are replaced by <c>***</c>. (2) Any 13–19 digit
/// run that passes the Luhn check (a PAN) inside a string value is masked to the last four.
/// The real rule is still "do not log it"; this only limits the blast radius.
/// </summary>
public sealed partial class SensitiveDataEnricher : ILogEventEnricher
{
    public const string Redacted = "***";

    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "NewPassword", "CurrentPassword", "Pan", "CardNumber", "Cvv", "Cvc",
        "Token", "AccessToken", "RefreshToken", "IdToken", "Authorization", "Secret", "ClientSecret", "PrivateKey"
    };

    [GeneratedRegex(@"(?<![0-9])(?:[0-9][ -]?){12,18}[0-9](?![0-9])")]
    private static partial Regex DigitRun();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var (name, value) in logEvent.Properties.ToArray())
        {
            var masked = SensitiveNames.Contains(name) ? new ScalarValue(Redacted) : Mask(value);
            if (!ReferenceEquals(masked, value))
                logEvent.AddOrUpdateProperty(new LogEventProperty(name, masked));
        }
    }

    /// <summary>Returns the same instance when nothing changed.</summary>
    public static LogEventPropertyValue Mask(LogEventPropertyValue value)
    {
        switch (value)
        {
            case ScalarValue { Value: string s }:
                var replaced = MaskPans(s);
                return ReferenceEquals(replaced, s) ? value : new ScalarValue(replaced);

            case StructureValue structure:
            {
                var changed = false;
                var props = structure.Properties.Select(p =>
                {
                    var inner = SensitiveNames.Contains(p.Name) ? new ScalarValue(Redacted) : Mask(p.Value);
                    changed |= !ReferenceEquals(inner, p.Value);
                    return new LogEventProperty(p.Name, inner);
                }).ToList();
                return changed ? new StructureValue(props, structure.TypeTag) : value;
            }

            case SequenceValue sequence:
            {
                var changed = false;
                var items = sequence.Elements.Select(e =>
                {
                    var inner = Mask(e);
                    changed |= !ReferenceEquals(inner, e);
                    return inner;
                }).ToList();
                return changed ? new SequenceValue(items) : value;
            }

            default:
                return value;
        }
    }

    public static string MaskPans(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var any = false;
        var result = DigitRun().Replace(input, match =>
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            if (digits.Length is < 13 or > 19 || !PassesLuhn(digits))
                return match.Value;
            any = true;
            return "****" + digits[^4..];
        });
        return any ? result : input;
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var doubleIt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleIt)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }

            sum += d;
            doubleIt = !doubleIt;
        }

        return sum % 10 == 0;
    }
}
