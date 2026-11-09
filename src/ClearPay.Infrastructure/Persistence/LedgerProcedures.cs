using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// Thin EF wrapper over the ledger read procedures (Persistence/Sql/*.sql).
/// SQL Server only: callers check <see cref="IsAvailable"/> and fall back to LINQ on SQLite (tests).
/// Parameters are typed <see cref="SqlParameter"/>s — no string concatenation.
/// </summary>
public sealed class LedgerProcedures
{
    private readonly ClearPayDbContext _db;

    public LedgerProcedures(ClearPayDbContext db) => _db = db;

    /// <summary>True when the ledger runs on SQL Server (the procs exist only there).</summary>
    public bool IsAvailable => _db.Database.IsSqlServer();

    public async Task<WalletTotalsRow> GetWalletSummaryAsync(
        Guid walletId,
        DateTimeOffset monthStart,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Database
            .SqlQueryRaw<WalletTotalsRow>(
                "EXEC dbo." + LedgerSqlScripts.WalletGetSummary + " @WalletId, @MonthStart",
                WalletParam(walletId),
                Offset("@MonthStart", monthStart))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.SingleOrDefault() ?? new WalletTotalsRow();
    }

    public Task<List<StatementRow>> GetStatementPageAsync(
        Guid walletId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? kind,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        _db.Database
            .SqlQueryRaw<StatementRow>(
                "EXEC dbo." + LedgerSqlScripts.WalletGetStatementPage
                    + " @WalletId, @From, @To, @Kind, @Offset, @PageSize",
                WalletParam(walletId),
                Offset("@From", from),
                Offset("@To", to),
                Int("@Kind", kind),
                Int("@Offset", offset),
                Int("@PageSize", pageSize))
            .ToListAsync(cancellationToken);

    public Task<List<DailyTotalsRow>> GetDailyTotalsAsync(
        DateTimeOffset dayStart,
        DateTimeOffset dayEnd,
        CancellationToken cancellationToken = default) =>
        _db.Database
            .SqlQueryRaw<DailyTotalsRow>(
                "EXEC dbo." + LedgerSqlScripts.LedgerGetDailyTotals + " @DayStart, @DayEnd",
                Offset("@DayStart", dayStart),
                Offset("@DayEnd", dayEnd))
            .ToListAsync(cancellationToken);

    private static SqlParameter WalletParam(Guid value) =>
        new("@WalletId", SqlDbType.UniqueIdentifier) { Value = value };

    private static SqlParameter Offset(string name, DateTimeOffset? value) =>
        new(name, SqlDbType.DateTimeOffset) { Value = value.HasValue ? value.Value : DBNull.Value };

    private static SqlParameter Int(string name, int? value) =>
        new(name, SqlDbType.Int) { Value = value.HasValue ? value.Value : DBNull.Value };
}

/// <summary>Result of <c>usp_Wallet_GetSummary</c>.</summary>
public sealed class WalletTotalsRow
{
    public decimal Balance { get; set; }

    public decimal MonthOutgoing { get; set; }

    public decimal MonthIncoming { get; set; }
}

/// <summary>Result row of <c>usp_Wallet_GetStatementPage</c>.</summary>
public sealed class StatementRow
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int Kind { get; set; }

    public decimal Amount { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid PairId { get; set; }

    public string? CounterpartyUserId { get; set; }

    public int TotalCount { get; set; }
}

/// <summary>Result row of <c>usp_Ledger_GetDailyTotals</c>. Net must be 0 for every kind.</summary>
public sealed class DailyTotalsRow
{
    public int Kind { get; set; }

    public long EntryCount { get; set; }

    public decimal CreditTotal { get; set; }

    public decimal DebitTotal { get; set; }

    public decimal Net { get; set; }
}
