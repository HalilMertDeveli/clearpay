namespace ClearPay.Application.Ports;

/// <summary>Account mail (reset links). Not a bank SMS gateway.</summary>
public interface IAccountMailer
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
