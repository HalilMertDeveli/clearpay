namespace ClearPay.Application.Identity;

public sealed class FirebaseTokenRequest
{
    public string IdToken { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? AccountKind { get; set; }
}
