using System.Collections;
using System.Globalization;
using System.Text;

namespace ClearPay.Messaging;

/// <summary>
/// Reads RabbitMQ's <c>x-death</c> header. The broker keeps one entry per (queue, reason) and bumps its
/// <c>count</c> each time the message is dead-lettered again. Our work queue only dead-letters on a
/// consumer nack, so "times rejected from Q" = failed attempts so far.
/// The client library hands strings back as <c>byte[]</c> and the array as <c>List&lt;object&gt;</c>.
/// </summary>
public static class DeathHeader
{
    public static long RejectedCount(IDictionary<string, object>? headers, string queue)
    {
        if (headers is null || !headers.TryGetValue("x-death", out var raw) || raw is not IEnumerable entries)
            return 0;

        long total = 0;
        foreach (var entry in entries)
        {
            if (entry is not IDictionary<string, object> death)
                continue;
            if (AsString(death, "queue") == queue
                && AsString(death, "reason") == "rejected"
                && death.TryGetValue("count", out var count))
            {
                total += Convert.ToInt64(count, CultureInfo.InvariantCulture);
            }
        }

        return total;
    }

    private static string? AsString(IDictionary<string, object> table, string key)
    {
        if (!table.TryGetValue(key, out var value))
            return null;
        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string s => s,
            _ => value?.ToString()
        };
    }
}
