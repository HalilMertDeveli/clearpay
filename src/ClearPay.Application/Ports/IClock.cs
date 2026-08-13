namespace ClearPay.Application.Ports;

/// <summary>ISP: time is not a wallet concern. Tests substitute a fixed clock.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
