namespace Gateway.Core.Policies;

public sealed record RateLimitDecision(
    bool Allowed,
    int Remaining,
    TimeSpan RetryAfter,
    int AppliedLimit,
    string Reason);
