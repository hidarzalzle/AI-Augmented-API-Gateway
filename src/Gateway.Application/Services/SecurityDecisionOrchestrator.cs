using Gateway.Application.Abstractions;
using Gateway.Application.Events;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;
using Microsoft.Extensions.Logging;

namespace Gateway.Application.Services;

public sealed class SecurityDecisionOrchestrator : ISecurityDecisionOrchestrator
{
    private readonly IAIClassifier _classifier;
    private readonly IAnomalyDetector _anomalyDetector;
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger<SecurityDecisionOrchestrator> _logger;

    public SecurityDecisionOrchestrator(
        IAIClassifier classifier,
        IAnomalyDetector anomalyDetector,
        IRateLimiter rateLimiter,
        ILogger<SecurityDecisionOrchestrator> logger)
    {
        _classifier = classifier;
        _anomalyDetector = anomalyDetector;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task<(TrafficClassificationResult Classification, AnomalyEvaluation Anomaly, Gateway.Core.Policies.RateLimitDecision RateLimit)> EvaluateAsync(
        TenantContext tenant,
        string routeKey,
        string requestFingerprint,
        string redactedPayload,
        long payloadSize,
        CancellationToken cancellationToken)
    {
        var classification = await _classifier.ClassifyAsync(tenant, requestFingerprint, redactedPayload, cancellationToken);
        var anomaly = await _anomalyDetector.EvaluateAsync(
            tenant,
            payloadSize,
            DateTimeOffset.UtcNow,
            classification,
            cancellationToken);

        var rateLimit = await _rateLimiter.EvaluateAsync(tenant, routeKey, anomaly.Score, cancellationToken);

        var evt = new SecurityDecisionEvent(
            requestFingerprint,
            tenant.TenantId,
            routeKey,
            anomaly.Score,
            classification.Category.ToString(),
            !rateLimit.Allowed,
            DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "security.decision {CorrelationId} {TenantId} {Route} {AnomalyScore} {TrafficCategory} {RateLimited}",
            evt.CorrelationId,
            evt.TenantId,
            evt.Route,
            evt.AnomalyScore,
            evt.TrafficCategory,
            evt.RateLimited);

        return (classification, anomaly, rateLimit);
    }
}
