using Gateway.Application.Services;
using Gateway.Core.Abstractions;
using Gateway.Core.Enums;
using Gateway.Core.Models;
using Gateway.Core.Policies;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Gateway.Tests;

public sealed class SecurityDecisionOrchestratorTests
{
    [Fact]
    public async Task EvaluateAsync_ReturnsRateLimitDecisionAndAnomaly()
    {
        var orchestrator = new SecurityDecisionOrchestrator(
            new StubClassifier(),
            new StubAnomalyDetector(),
            new StubRateLimiter(),
            NullLogger<SecurityDecisionOrchestrator>.Instance);

        var (classification, anomaly, rateLimit) = await orchestrator.EvaluateAsync(
            new TenantContext("tenant-a", "standard", new Dictionary<string, string>()),
            "/orders",
            "corr-1",
            "payload",
            1200,
            CancellationToken.None);

        Assert.Equal(TrafficCategory.Normal, classification.Category);
        Assert.True(anomaly.Score > 0);
        Assert.True(rateLimit.Allowed);
    }

    private sealed class StubClassifier : IAIClassifier
    {
        public Task<TrafficClassificationResult> ClassifyAsync(TenantContext tenant, string requestFingerprint, string redactedPayload, CancellationToken cancellationToken)
            => Task.FromResult(new TrafficClassificationResult(TrafficCategory.Normal, 0.8, "test", new Dictionary<string, string>()));
    }

    private sealed class StubAnomalyDetector : IAnomalyDetector
    {
        public ValueTask<AnomalyEvaluation> EvaluateAsync(TenantContext tenant, long payloadSizeBytes, DateTimeOffset requestTimestamp, TrafficClassificationResult classification, CancellationToken cancellationToken)
            => ValueTask.FromResult(new AnomalyEvaluation(false, 0.15, new[] { "test_signal" }));
    }

    private sealed class StubRateLimiter : IRateLimiter
    {
        public Task<RateLimitDecision> EvaluateAsync(TenantContext tenant, string routeKey, double anomalyScore, CancellationToken cancellationToken)
            => Task.FromResult(new RateLimitDecision(true, 80, TimeSpan.Zero, 100, "allowed"));
    }
}
