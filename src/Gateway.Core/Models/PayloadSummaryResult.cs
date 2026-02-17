namespace Gateway.Core.Models;

public sealed record PayloadSummaryResult(
    bool WasSummarized,
    string? Summary,
    string? SecureReference);
