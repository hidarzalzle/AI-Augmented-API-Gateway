using Gateway.Core.Models;

namespace Gateway.Core.Abstractions;

public interface IPayloadSummarizer
{
    Task<PayloadSummaryResult> SummarizeAsync(
        TenantContext tenant,
        string redactedPayload,
        int thresholdBytes,
        CancellationToken cancellationToken);
}
