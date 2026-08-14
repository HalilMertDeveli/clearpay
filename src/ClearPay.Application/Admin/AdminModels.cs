namespace ClearPay.Application.Admin;

public sealed record FailedOutboxItem(Guid Id, string Type, Guid CorrelationId, DateTimeOffset OccurredAt, string Payload);

public sealed record AuditItem(Guid Id, string ActorUserId, string Action, Guid CorrelationId, DateTimeOffset CreatedAt, string? Details);
