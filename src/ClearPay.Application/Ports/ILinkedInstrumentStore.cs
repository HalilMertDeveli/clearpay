using ClearPay.Application.Funding;

namespace ClearPay.Application.Ports;

public interface ILinkedInstrumentStore
{
    Task<IReadOnlyList<LinkedInstrumentDto>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <returns>The new row, or null if SQL is down / last4 invalid / duplicate.</returns>
    Task<LinkedInstrumentDto?> AddAsync(
        string userId,
        string last4,
        string label,
        CancellationToken cancellationToken = default);
}
