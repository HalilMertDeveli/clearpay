namespace ClearPay.Tests;

/// <summary>
/// Fact that needs a real SQL Server (stored procedures do not exist on SQLite).
/// Runs when <see cref="Variable"/> holds a connection string to a THROWAWAY server
/// (CI service container, or the Docker container from scripts/perf/run-perf.ps1);
/// otherwise it is reported as skipped with the reason — never as a silent pass.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string Variable = "CLEARPAY_TEST_SQLSERVER";

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
            Skip = $"Set {Variable} to a throwaway SQL Server connection string to run stored procedure tests.";
    }
}
