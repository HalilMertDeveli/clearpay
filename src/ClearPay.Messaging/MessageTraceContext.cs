using System.Diagnostics;
using System.Text;
using OpenTelemetry.Context.Propagation;

namespace ClearPay.Messaging;

/// <summary>
/// Carries the W3C <c>traceparent</c> / <c>tracestate</c> through RabbitMQ message headers so a
/// consumer span becomes a child of the HTTP request that wrote the outbox row.
/// RabbitMQ.Client delivers string headers back as <c>byte[]</c>, so <see cref="Extract"/> accepts both.
/// (Written by the observability team in Infrastructure; moved here so the worker services
/// can use it without referencing the web app's Infrastructure project.)
/// </summary>
public static class MessageTraceContext
{
    private static readonly TraceContextPropagator Propagator = new();

    /// <summary>Writes the current (or given) activity context into <paramref name="headers"/>.</summary>
    public static void Inject(Activity? activity, IDictionary<string, object> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        if (activity is null)
            return;

        Propagator.Inject(
            new PropagationContext(activity.Context, default),
            headers,
            static (carrier, key, value) => carrier[key] = value);
    }

    /// <summary>Reads a parent context from message headers; returns default when absent or malformed.</summary>
    public static ActivityContext Extract(IDictionary<string, object>? headers)
    {
        if (headers is null || headers.Count == 0)
            return default;

        var context = Propagator.Extract(default, headers, static (carrier, key) =>
        {
            if (!carrier.TryGetValue(key, out var raw) || raw is null)
                return Array.Empty<string>();
            return raw switch
            {
                byte[] bytes => new[] { Encoding.UTF8.GetString(bytes) },
                string s => new[] { s },
                _ => Array.Empty<string>()
            };
        });
        return context.ActivityContext;
    }

    /// <summary>
    /// Starts a producer span and stamps its context into <paramref name="headers"/>.
    /// <paramref name="parentTraceParent"/> is the <c>traceparent</c> saved on the outbox row: the outbox
    /// worker runs later on a Hangfire thread, so without it the publish span would start a new trace.
    /// </summary>
    public static Activity? StartPublish(
        string destination,
        string messageType,
        IDictionary<string, object> headers,
        string? parentTraceParent = null)
    {
        ArgumentNullException.ThrowIfNull(headers);
        Activity? activity;
        if (parentTraceParent is not null && ActivityContext.TryParse(parentTraceParent, null, out var parent))
            activity = MessagingTelemetry.Source.StartActivity($"{destination} publish", ActivityKind.Producer, parent);
        else
            activity = MessagingTelemetry.Source.StartActivity($"{destination} publish", ActivityKind.Producer);

        if (activity is not null)
        {
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination.name", destination);
            activity.SetTag("messaging.operation", "publish");
            activity.SetTag("clearpay.message_type", messageType);
            Inject(activity, headers);
        }
        else if (Activity.Current is not null)
        {
            Inject(Activity.Current, headers);
        }
        else if (parentTraceParent is not null)
        {
            // Nobody listens to the source (tracing off): still forward the original request's context.
            headers["traceparent"] = parentTraceParent;
        }

        return activity;
    }

    /// <summary>Starts a consumer span whose parent is the context carried in <paramref name="headers"/>.</summary>
    public static Activity? StartConsume(string destination, string messageType, IDictionary<string, object>? headers)
    {
        var parent = Extract(headers);
        var activity = MessagingTelemetry.Source.StartActivity(
            $"{destination} process", ActivityKind.Consumer, parent);
        if (activity is not null)
        {
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination.name", destination);
            activity.SetTag("messaging.operation", "process");
            activity.SetTag("clearpay.message_type", messageType);
        }

        return activity;
    }
}
