namespace ClearPay.Application.Ports;

/// <summary>Verifies a Firebase Auth ID token. Wallet stays SQL — this is identity only.</summary>
public interface IFirebaseIdTokenVerifier
{
    bool IsConfigured { get; }

    Task<FirebasePrincipal?> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed record FirebasePrincipal(string Uid, string Email);
