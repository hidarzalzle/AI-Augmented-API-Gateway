using Gateway.Core.Abstractions;
using Gateway.Core.Models;

namespace Gateway.Infrastructure.Services;

public sealed class PayloadSummarizer : IPayloadSummarizer
{
    public Task<PayloadSummaryResult> SummarizeAsync(TenantContext tenant, string redactedPayload, int thresholdBytes, CancellationToken cancellationToken)
    {
        if (redactedPayload.Length <= thresholdBytes)
        {
            return Task.FromResult(new PayloadSummaryResult(false, null, null));
        }

        var summary = redactedPayload[..Math.Min(300, redactedPayload.Length)] + "...";
        var secureRef = $"vault://payloads/{tenant.TenantId}/{Guid.NewGuid():N}";
        return Task.FromResult(new PayloadSummaryResult(true, summary, secureRef));
    }
}
