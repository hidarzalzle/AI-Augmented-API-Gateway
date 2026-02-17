using System.Collections.Concurrent;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;
using Gateway.Core.Policies;

namespace Gateway.Infrastructure.Services;

public sealed class AdaptiveRedisLikeRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();

    public Task<RateLimitDecision> EvaluateAsync(TenantContext tenant, string routeKey, double anomalyScore, CancellationToken cancellationToken)
    {
        var baseLimit = tenant.Plan.Equals("enterprise", StringComparison.OrdinalIgnoreCase) ? 500 : 100;
        var adjustedLimit = (int)Math.Max(10, baseLimit * (1 - Math.Min(0.7, anomalyScore)));

        var key = $"{tenant.TenantId}:{routeKey}";
        var counter = _counters.GetOrAdd(key, _ => new SlidingWindowCounter());
        var allowed = counter.TryConsume(adjustedLimit, TimeSpan.FromMinutes(1), out var remaining, out var retryAfter);

        var decision = new RateLimitDecision(
            allowed,
            remaining,
            retryAfter,
            adjustedLimit,
            allowed ? "allowed" : "sliding_window_exceeded");

        return Task.FromResult(decision);
    }

    private sealed class SlidingWindowCounter
    {
        private readonly Queue<DateTimeOffset> _events = new();
        private readonly object _lock = new();

        public bool TryConsume(int limit, TimeSpan window, out int remaining, out TimeSpan retryAfter)
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow;
                while (_events.Count > 0 && (now - _events.Peek()) > window)
                {
                    _events.Dequeue();
                }

                if (_events.Count >= limit)
                {
                    var oldest = _events.Peek();
                    retryAfter = window - (now - oldest);
                    remaining = 0;
                    return false;
                }

                _events.Enqueue(now);
                remaining = Math.Max(0, limit - _events.Count);
                retryAfter = TimeSpan.Zero;
                return true;
            }
        }
    }
}
