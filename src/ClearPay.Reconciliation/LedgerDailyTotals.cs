using System.Data;
using Microsoft.Data.SqlClient;

namespace ClearPay.Reconciliation;

public sealed record LedgerKindTotal(string Kind, long EntryCount, decimal CreditTotal, decimal DebitTotal, decimal Net);

/// <summary>
/// Wallet-ledger integrity check for the same business day. It calls the T-SQL team's
/// <c>dbo.usp_Ledger_GetDailyTotals</c> (one index range seek): every wallet movement is a +/- pair,
/// so a kind whose Net is not zero means a half-written pair.
/// This is the only place the service touches the web app's database, and only through EXECUTE on one
/// stored procedure with its own connection string (in production a login with just that GRANT).
/// </summary>
public interface ILedgerDailyTotals
{
    /// <summary>Null when the check is not configured.</summary>
    Task<IReadOnlyList<LedgerKindTotal>?> ReadAsync(DateTimeOffset dayStart, DateTimeOffset dayEnd, CancellationToken cancellationToken);
}

public sealed class SqlLedgerDailyTotals : ILedgerDailyTotals
{
    private readonly string? _connectionString;

    public SqlLedgerDailyTotals(string? connectionString)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    }

    public async Task<IReadOnlyList<LedgerKindTotal>?> ReadAsync(
        DateTimeOffset dayStart,
        DateTimeOffset dayEnd,
        CancellationToken cancellationToken)
    {
        if (_connectionString is null)
            return null;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new SqlCommand("dbo.usp_Ledger_GetDailyTotals", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };
        command.Parameters.Add(new SqlParameter("@DayStart", SqlDbType.DateTimeOffset) { Value = dayStart });
        command.Parameters.Add(new SqlParameter("@DayEnd", SqlDbType.DateTimeOffset) { Value = dayEnd });

        var totals = new List<LedgerKindTotal>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            totals.Add(new LedgerKindTotal(
                KindName(reader.GetValue(reader.GetOrdinal("Kind"))),
                reader.GetInt64(reader.GetOrdinal("EntryCount")),
                reader.GetDecimal(reader.GetOrdinal("CreditTotal")),
                reader.GetDecimal(reader.GetOrdinal("DebitTotal")),
                reader.GetDecimal(reader.GetOrdinal("Net"))));
        }

        return totals;
    }

    /// <summary>LedgerEntry.Kind is stored as int (Domain <c>LedgerEntryKind</c>); this service does not reference Domain.</summary>
    internal static string KindName(object value) => Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture) switch
    {
        0 => "Transfer",
        1 => "TopUp",
        2 => "Withdraw",
        3 => "Refund",
        var other => "Kind" + other.ToString(System.Globalization.CultureInfo.InvariantCulture)
    };
}
