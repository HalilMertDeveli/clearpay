using ClearPay.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace ClearPay.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Bireysel or Kurumsal chrome. Same ledger; not a satıcı panel.</summary>
    public string AccountKind { get; set; } = AccountKinds.Bireysel;
}
