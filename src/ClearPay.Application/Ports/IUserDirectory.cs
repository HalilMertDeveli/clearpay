namespace ClearPay.Application.Ports;

/// <summary>ISP: resolve Identity email → user id without PageModels taking UserManager.</summary>
public interface IUserDirectory
{
    Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<string?> FindEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
