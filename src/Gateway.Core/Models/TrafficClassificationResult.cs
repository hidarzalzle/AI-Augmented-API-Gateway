using Gateway.Core.Enums;

namespace Gateway.Core.Models;

public sealed record TrafficClassificationResult(
    TrafficCategory Category,
    double Confidence,
    string ModelVersion,
    IReadOnlyDictionary<string, string> Tags);
