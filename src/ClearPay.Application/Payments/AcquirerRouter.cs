namespace ClearPay.Application.Payments;

/// <summary>Config: <c>Payments:Routing</c>. BIN prefix → adapter name; longest prefix wins.</summary>
public sealed class PaymentRoutingOptions
{
    public const string SectionName = "Payments:Routing";

    public string DefaultProvider { get; set; } = "MockBankA";

    public Dictionary<string, string> BinRoutes { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Picks the acquirer for a card. Simple on purpose: longest matching BIN prefix from config,
/// else the default. Adding a bank = one adapter class + one config line, no switch in the gateway.
/// </summary>
public sealed class AcquirerRouter
{
    private readonly Dictionary<string, IAcquirerAdapter> _adapters;
    private readonly PaymentRoutingOptions _options;

    public AcquirerRouter(IEnumerable<IAcquirerAdapter> adapters, PaymentRoutingOptions options)
    {
        ArgumentNullException.ThrowIfNull(adapters);
        ArgumentNullException.ThrowIfNull(options);
        _adapters = adapters.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
        _options = options;

        foreach (var target in _options.BinRoutes.Values.Append(_options.DefaultProvider))
        {
            if (!_adapters.ContainsKey(target))
                throw new InvalidOperationException($"Payments routing points at unknown adapter '{target}'.");
        }
    }

    public IReadOnlyCollection<string> Names => _adapters.Keys;

    public IAcquirerAdapter ForBin(string cardBin)
    {
        var bin = cardBin ?? string.Empty;
        var match = _options.BinRoutes
            .Where(route => route.Key.Length > 0 && bin.StartsWith(route.Key, StringComparison.Ordinal))
            .OrderByDescending(route => route.Key.Length)
            .Select(route => route.Value)
            .FirstOrDefault();
        return _adapters[match ?? _options.DefaultProvider];
    }

    /// <summary>Capture/refund/void must go back to the acquirer that authorized the payment.</summary>
    public IAcquirerAdapter ByName(string provider) =>
        _adapters.TryGetValue(provider, out var adapter)
            ? adapter
            : throw new InvalidOperationException($"No acquirer adapter named '{provider}'.");
}
