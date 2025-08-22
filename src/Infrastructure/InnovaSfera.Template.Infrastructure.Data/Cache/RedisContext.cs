using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace InnovaSfera.Template.Infrastructure.Data.Cache;

public class RedisContext
{
    private static Lazy<ConnectionMultiplexer>? _lazyConnection;
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
        _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var connectionString = _configuration["REDIS_CONNECTION_STRING"]?.ToString();
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Redis connection string is not configured.");
            return ConnectionMultiplexer.Connect(connectionString);
        });
    }

    public static ConnectionMultiplexer Connection => 
        _lazyConnection?.Value ?? throw new InvalidOperationException("RedisContext not initialized. Call Initialize() first.");
}
