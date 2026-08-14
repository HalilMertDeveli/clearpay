using Microsoft.Data.SqlClient;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>SQL Server down / timeout — not a unique-key race. Sqlite test failures stay loud.</summary>
internal static class LedgerUnavailable
{
    public static bool Matches(Exception exception)
    {
        for (var e = exception; e is not null; e = e.InnerException)
        {
            if (e is SqlException or TimeoutException)
                return true;
        }

        return false;
    }
}
