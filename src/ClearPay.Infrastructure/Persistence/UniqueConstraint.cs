using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

internal static class UniqueConstraint
{
    public static bool IsDuplicateKey(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception.InnerException is SqliteException sqlite && sqlite.SqliteErrorCode == 19)
            return true;
        if (exception.InnerException is SqlException sql && (sql.Number is 2627 or 2601))
            return true;

        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }
}
