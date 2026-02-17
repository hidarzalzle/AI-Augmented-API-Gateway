namespace Gateway.Application.Events;

public sealed record SecurityDecisionEvent(
    string CorrelationId,
    string TenantId,
    string Route,
    double AnomalyScore,
    string TrafficCategory,
    bool RateLimited,
    DateTimeOffset TimestampUtc);
