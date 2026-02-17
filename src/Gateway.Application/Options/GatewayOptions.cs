namespace Gateway.Application.Options;

public sealed class GatewayOptions
{
    public int MaxRequestBytes { get; set; } = 1_000_000;
    public int SummarizationThresholdBytes { get; set; } = 30_000;
    public int AiTimeoutMs { get; set; } = 800;
    public string DownstreamBaseUrl { get; set; } = "http://localhost:9000";
}
