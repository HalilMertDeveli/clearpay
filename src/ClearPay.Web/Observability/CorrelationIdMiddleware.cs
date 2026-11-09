using System.Diagnostics;
using Serilog.Context;

namespace ClearPay.Web.Observability;

/// <summary>
/// Every response carries <c>X-Correlation-Id</c> and <c>X-Trace-Id</c>. A support ticket that quotes
/// either one is enough to find the request in Seq and the trace in Tempo (docs/OBSERVABILITY.md).
/// An incoming <c>X-Correlation-Id</c> is kept only if it is short and URL-safe (no log injection).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string TraceHeaderName = "X-Trace-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var incoming = context.Request.Headers[HeaderName].ToString();
        var correlationId = IsSafe(incoming) ? incoming : traceId ?? context.TraceIdentifier;

        activity?.SetTag("clearpay.correlation_id", correlationId);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            if (traceId is not null)
                context.Response.Headers[TraceHeaderName] = traceId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    public static bool IsSafe(string? value) =>
        !string.IsNullOrEmpty(value)
        && value.Length <= 64
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
}
