using System.Text.Json;
using StackExchange.Redis;

namespace MAF.Part05.Persistence;

/// <summary>
/// Part 5: Redis Thread Persistence in .NET
/// </summary>
public class RedisThreadStore
{
    private readonly IDatabase _redis;
    private readonly TimeSpan _ttl = TimeSpan.FromDays(7);

    public RedisThreadStore(string connectionString = "localhost:6379")
    {
        var connection = ConnectionMultiplexer.Connect(connectionString);
        _redis = connection.GetDatabase();
    }

    public async Task SaveThreadAsync(string sessionId, object thread)
    {
        var data = new
        {
            Messages = GetMessages(thread),
            Metadata = new
            {
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            }
        };

        var json = JsonSerializer.Serialize(data);
        await _redis.StringSetAsync($"agent:thread:{sessionId}", json, _ttl);
        Console.WriteLine($"Thread saved: {sessionId}");
    }

    public async Task<object?> LoadThreadAsync(string sessionId, dynamic agent)
    {
        var data = await _redis.StringGetAsync($"agent:thread:{sessionId}");

        if (data.HasValue)
        {
            var parsed = JsonSerializer.Deserialize<ThreadData>(data!);
            var thread = agent.GetNewThread();
            // Restore messages to thread
            Console.WriteLine($"Thread loaded: {sessionId}");
            return thread;
        }

        Console.WriteLine($"No thread found for {sessionId}, creating new");
        return agent.GetNewThread();
    }

    public async Task<bool> DeleteThreadAsync(string sessionId)
    {
        return await _redis.KeyDeleteAsync($"agent:thread:{sessionId}");
    }

    public async Task<List<string>> ListSessionsAsync(string pattern = "agent:thread:*")
    {
        var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints()[0]);
        var keys = server.Keys(pattern: pattern);
        return keys.Select(k => k.ToString().Split(':').Last()).ToList();
    }

    private static List<object> GetMessages(object thread)
    {
        // Extract messages from thread
        var prop = thread.GetType().GetProperty("Messages");
        return prop?.GetValue(thread) as List<object> ?? new List<object>();
    }

    private record ThreadData(List<object> Messages, object Metadata);
}
