using ClearPay.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClearPay.Notifications;

/// <summary>
/// The notification service's own tables, in the <c>notif</c> schema. Today it shares the SQL Server
/// instance (and database) with the web app to keep the demo to one container; it never reads or writes
/// the web app's <c>dbo</c> tables, so moving it to its own database is a connection-string change.
/// </summary>
public sealed class NotificationsDbContext : DbContext
{
    public const string Schema = "notif";

    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite (tests) has no schemas; EF ignores the schema there.
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddProcessedMessages(Schema);
        modelBuilder.Entity<NotificationLog>(b =>
        {
            b.ToTable("NotificationLog");
            b.HasKey(n => n.Id);
            b.Property(n => n.EventType).IsRequired().HasMaxLength(128);
            b.Property(n => n.Channel).IsRequired().HasMaxLength(16);
            b.Property(n => n.Recipient).IsRequired().HasMaxLength(256);
            b.Property(n => n.Subject).IsRequired().HasMaxLength(512);
            b.HasIndex(n => n.MessageId).HasDatabaseName("IX_NotificationLog_MessageId");
            b.HasIndex(n => n.CreatedAt).HasDatabaseName("IX_NotificationLog_CreatedAt");
        });
    }
}

/// <summary>One fake e-mail / SMS / webhook "sent" for one event. Proof of delivery for the demo and tests.</summary>
public sealed class NotificationLog
{
    public Guid Id { get; set; }

    /// <summary>The RabbitMQ MessageId (= web outbox row id) that caused it. A real provider gets it as idempotency key.</summary>
    public Guid MessageId { get; set; }

    public string EventType { get; set; } = string.Empty;

    /// <summary>Email, Sms or Webhook.</summary>
    public string Channel { get; set; } = string.Empty;

    public string Recipient { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>For <c>dotnet ef migrations add</c>; the connection string is never opened.</summary>
public sealed class NotificationsDesignTimeFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=ClearPay;Integrated Security=True;TrustServerCertificate=True",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema))
            .Options);
}
