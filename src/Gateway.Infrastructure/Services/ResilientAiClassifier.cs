using Gateway.Core.Abstractions;
using Gateway.Core.Enums;
using Gateway.Core.Models;
using Gateway.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Infrastructure.Services;

public sealed class ResilientAiClassifier : IAIClassifier
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResilientAiClassifier> _logger;
    private readonly AiProviderOptions _options;
    private int _consecutiveFailures;
    private DateTimeOffset _openUntil = DateTimeOffset.MinValue;

    public ResilientAiClassifier(IMemoryCache cache, IOptions<AiProviderOptions> options, ILogger<ResilientAiClassifier> logger)
    {
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<TrafficClassificationResult> ClassifyAsync(TenantContext tenant, string requestFingerprint, string redactedPayload, CancellationToken cancellationToken)
    {
        var cacheKey = $"ai:{tenant.TenantId}:{requestFingerprint}:{redactedPayload.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is TrafficClassificationResult cached)
        {
            return cached;
        }

        if (_openUntil > DateTimeOffset.UtcNow)
        {
            return Fallback();
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.TimeoutMs);

        try
        {
            await Task.Delay(30, timeoutCts.Token);
            var suspicious = redactedPayload.Contains("DROP TABLE", StringComparison.OrdinalIgnoreCase) || redactedPayload.Length > 100_000;

            var result = new TrafficClassificationResult(
                suspicious ? TrafficCategory.Suspicious : TrafficCategory.Normal,
                suspicious ? 0.91 : 0.72,
                "stub-v1",
                new Dictionary<string, string> { ["provider"] = _options.Provider });

            using (var entry = _cache.CreateEntry(cacheKey))
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                entry.Value = result;
            }
            _consecutiveFailures = 0;
            return result;
        }
        catch (OperationCanceledException)
        {
            _consecutiveFailures++;
            _logger.LogWarning("AI classifier timeout/failure. failureCount={FailureCount}", _consecutiveFailures);
            if (_consecutiveFailures >= _options.CircuitBreakerThreshold)
            {
                _openUntil = DateTimeOffset.UtcNow.AddSeconds(_options.CircuitBreakerOpenSeconds);
            }

            return Fallback();
        }
    }

    private static TrafficClassificationResult Fallback() =>
        new(TrafficCategory.Unknown, 0.0, "fallback-v1", new Dictionary<string, string> { ["fallback"] = "true" });
}
