using Gateway.Core.Abstractions;
using Gateway.Core.Enums;
using Gateway.Core.Models;

namespace Gateway.Core.Services;

public sealed class DefaultAnomalyDetector : IAnomalyDetector
{
    public ValueTask<AnomalyEvaluation> EvaluateAsync(
        TenantContext tenant,
        long payloadSizeBytes,
        DateTimeOffset requestTimestamp,
        TrafficClassificationResult classification,
        CancellationToken cancellationToken)
    {
        var signals = new List<string>();
        var score = 0.0;

        if (payloadSizeBytes > 256_000)
        {
            score += 0.35;
            signals.Add("payload_size_abnormal");
        }

        if (classification.Category is TrafficCategory.Suspicious)
        {
            score += 0.45;
            signals.Add("ai_suspicious_category");
        }

        if (requestTimestamp.Hour is >= 0 and <= 4)
        {
            score += 0.1;
            signals.Add("off_hours_access_pattern");
        }

        if (classification.Confidence < 0.35)
        {
            score += 0.1;
            signals.Add("low_classification_confidence");
        }

        var bounded = Math.Min(1.0, score);
        return ValueTask.FromResult(new AnomalyEvaluation(bounded >= 0.6, bounded, signals));
    }
}
