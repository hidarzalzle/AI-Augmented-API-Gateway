using Gateway.Core.Models;
using Gateway.Core.Policies;

namespace Gateway.Application.Abstractions;

public interface ISecurityDecisionOrchestrator
{
    Task<(TrafficClassificationResult Classification, AnomalyEvaluation Anomaly, RateLimitDecision RateLimit)> EvaluateAsync(
        TenantContext tenant,
        string routeKey,
        string requestFingerprint,
        string redactedPayload,
        long payloadSize,
        CancellationToken cancellationToken);
}
