using Gateway.Core.Models;

namespace Gateway.Core.Abstractions;

public interface IAIClassifier
{
    Task<TrafficClassificationResult> ClassifyAsync(
        TenantContext tenant,
        string requestFingerprint,
        string redactedPayload,
        CancellationToken cancellationToken);
}
