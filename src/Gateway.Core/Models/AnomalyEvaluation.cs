namespace Gateway.Core.Models;

public sealed record AnomalyEvaluation(
    bool IsAnomalous,
    double Score,
    IReadOnlyCollection<string> Signals);
