using Gateway.Core.Models;
using Gateway.Core.Policies;

namespace Gateway.Core.Abstractions;

public interface IRateLimiter
{
    Task<RateLimitDecision> EvaluateAsync(
        TenantContext tenant,
        string routeKey,
        double anomalyScore,
        CancellationToken cancellationToken);
}
