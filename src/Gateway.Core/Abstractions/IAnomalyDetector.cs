using Gateway.Core.Models;

namespace Gateway.Core.Abstractions;

public interface IAnomalyDetector
{
    ValueTask<AnomalyEvaluation> EvaluateAsync(
        TenantContext tenant,
        long payloadSizeBytes,
        DateTimeOffset requestTimestamp,
        TrafficClassificationResult classification,
        CancellationToken cancellationToken);
}
