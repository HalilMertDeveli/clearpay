namespace ClearPay.Application.Ports;

/// <summary>JSON API identity (ARCHITECTURE: cookie is not the money-API primary scheme).</summary>
public interface IJwtTokenIssuer
{
    string Issue(string userId, string email, IReadOnlyList<string> roles);
}
