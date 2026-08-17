namespace ClearPay.Application.Identity;

public sealed class ForgotPasswordRequest
{
    public string? Email { get; set; }

    public string? Phone { get; set; }
}
