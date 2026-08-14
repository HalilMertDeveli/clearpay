namespace ClearPay.Web.Api;

public sealed class TokenRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
