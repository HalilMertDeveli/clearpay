using StackExchange.Redis;

namespace ClearPay.Infrastructure.Caching;

internal static class RedisMultiplexerFactory
{
    public static IConnectionMultiplexer? TryCreate(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 1000;
            options.AsyncTimeout = 1000;
            options.SyncTimeout = 1000;
            options.ConnectRetry = 0;
            return ConnectionMultiplexer.Connect(options);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
