using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace ClearPay.Web.Observability;

/// <summary>
/// Adds <c>TraceId</c> / <c>SpanId</c> as normal properties so they show up in the console template
/// and are searchable in Seq (<c>TraceId = '...'</c>) next to Seq's own trace view.
/// </summary>
public sealed class TraceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
            return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToHexString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToHexString()));
    }
}
