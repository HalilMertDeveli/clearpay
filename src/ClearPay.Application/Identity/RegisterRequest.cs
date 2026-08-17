namespace ClearPay.Application.Identity;

public sealed class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Optional. Bireysel (default) or Kurumsal. Same 8 screens.</summary>
    public string AccountKind { get; set; } = string.Empty;
}
