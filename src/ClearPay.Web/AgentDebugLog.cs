using System.Text.Json;

internal static class AgentDebugLog
{
    private const string Path = @"D:\ClearPay\clearpay\debug-021de0.log";

    public static void Write(string hypothesisId, string location, string message, object data)
    {
        // #region agent log
        try
        {
            var line = JsonSerializer.Serialize(new
            {
                sessionId = "021de0",
                runId = "post-fix",
                hypothesisId,
                location,
                message,
                data,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            File.AppendAllText(Path, line + Environment.NewLine);
        }
        catch
        {
            /* debug ingest must not break the demo host */
        }
        // #endregion
    }
}
