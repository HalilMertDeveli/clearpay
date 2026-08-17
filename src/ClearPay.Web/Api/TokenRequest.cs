namespace ClearPay.Web.Api;

public sealed class TokenRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Optional picker value stored on the user so JWT round-trips AccountKind.</summary>
    public string? AccountKind { get; set; }
}
