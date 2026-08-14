using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Identity;

namespace ClearPay.Infrastructure.Identity;

public sealed class IdentityUserDirectory : IUserDirectory
{
    private readonly UserManager<ApplicationUser> _users;

    public IdentityUserDirectory(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public async Task<string?> FindUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var user = await _users.FindByEmailAsync(email.Trim()).ConfigureAwait(false);
        return user?.Id;
    }

    public async Task<string?> FindEmailByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _users.FindByIdAsync(userId).ConfigureAwait(false);
        return user?.Email;
    }
}
