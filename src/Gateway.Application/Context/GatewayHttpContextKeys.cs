namespace Gateway.Application.Context;

public static class GatewayHttpContextKeys
{
    public const string CorrelationId = "gateway.correlation-id";
    public const string Tenant = "gateway.tenant";
    public const string RedactedPayload = "gateway.redacted-payload";
    public const string Classification = "gateway.classification";
    public const string Anomaly = "gateway.anomaly";
    public const string PayloadSummary = "gateway.payload-summary";
}
