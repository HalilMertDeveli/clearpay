namespace ClearPay.Infrastructure;

/// <summary>Local SQL Server (Docker Compose). DbContext arrives in TASK-04.</summary>
public sealed class SqlOptions
{
    public const string SectionName = "ConnectionStrings";

    public string ClearPay { get; init; } = string.Empty;
}
