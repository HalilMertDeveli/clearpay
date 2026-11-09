using System.Reflection;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// T-SQL stored procedures shipped as embedded <c>Persistence/Sql/*.sql</c> files.
/// Each file is one idempotent <c>CREATE OR ALTER PROCEDURE</c>; the
/// <c>AddLedgerReadProcedures</c> migration runs them. SQL Server only (SQLite tests use LINQ).
/// Numbers and plans: docs/SQL-PERF.md.
/// </summary>
public static class LedgerSqlScripts
{
    public const string WalletGetSummary = "usp_Wallet_GetSummary";

    public const string WalletGetStatementPage = "usp_Wallet_GetStatementPage";

    public const string LedgerGetDailyTotals = "usp_Ledger_GetDailyTotals";

    public static IReadOnlyList<string> All { get; } =
    [
        WalletGetSummary,
        WalletGetStatementPage,
        LedgerGetDailyTotals
    ];

    public static string Read(string procedureName)
    {
        var assembly = typeof(LedgerSqlScripts).Assembly;
        var suffix = ".Persistence.Sql." + procedureName + ".sql";
        var resource = assembly.GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(suffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Embedded SQL script '{procedureName}.sql' not found.");

        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
