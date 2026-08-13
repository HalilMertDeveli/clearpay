namespace ClearPay.Infrastructure;

/// <summary>Local SQL Server (Docker Compose). Bound in AddClearPay from ConnectionStrings:ClearPay.</summary>
public sealed class SqlOptions
{
    public const string SectionName = "ConnectionStrings";

    public string ClearPay { get; init; } = string.Empty;
}
