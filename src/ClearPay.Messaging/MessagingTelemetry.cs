using System.Diagnostics;

namespace ClearPay.Messaging;

/// <summary>
/// The one <see cref="ActivitySource"/> for publish / process spans. The web app's
/// <c>ClearPayTelemetry.Messaging</c> points at this same instance; services add it with
/// <c>AddSource(MessagingTelemetry.SourceName)</c>.
/// </summary>
public static class MessagingTelemetry
{
    public const string SourceName = "ClearPay.Messaging";

    public static readonly ActivitySource Source = new(SourceName);
}
