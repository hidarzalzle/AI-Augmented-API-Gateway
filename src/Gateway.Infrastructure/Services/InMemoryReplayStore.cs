using System.Collections.Concurrent;
using Gateway.Core.Abstractions;

namespace Gateway.Infrastructure.Services;

public sealed class InMemoryReplayStore : IWebhookReplayStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _cache = new();

    public Task<bool> TryRegisterAsync(string tenantId, string replayId, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var key = $"{tenantId}:{replayId}";
        var now = DateTimeOffset.UtcNow;
        var expiry = now.Add(ttl);

        foreach (var entry in _cache.Where(x => x.Value <= now).ToArray())
        {
            _cache.TryRemove(entry.Key, out _);
        }

        return Task.FromResult(_cache.TryAdd(key, expiry));
    }
}
