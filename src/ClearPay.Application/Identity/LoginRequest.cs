namespace ClearPay.Application.Identity;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Demo TCKN. Mapped server-side; Mernis yok.</summary>
    public string? Tc { get; set; }

    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
