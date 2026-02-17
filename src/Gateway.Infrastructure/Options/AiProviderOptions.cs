namespace Gateway.Infrastructure.Options;

public sealed class AiProviderOptions
{
    public string Provider { get; set; } = "Stub";
    public int TimeoutMs { get; set; } = 800;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerOpenSeconds { get; set; } = 30;
}
