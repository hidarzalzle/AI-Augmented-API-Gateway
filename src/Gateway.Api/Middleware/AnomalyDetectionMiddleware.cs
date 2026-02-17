using Gateway.Application.Context;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;

namespace Gateway.Api.Middleware;

public sealed class AnomalyDetectionMiddleware : IMiddleware
{
    private readonly IAnomalyDetector _anomalyDetector;

    public AnomalyDetectionMiddleware(IAnomalyDetector anomalyDetector)
    {
        _anomalyDetector = anomalyDetector;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenant = (TenantContext)context.Items[GatewayHttpContextKeys.Tenant]!;
        var classification = (TrafficClassificationResult)context.Items[GatewayHttpContextKeys.Classification]!;
        var payload = context.Items[GatewayHttpContextKeys.RedactedPayload]?.ToString() ?? string.Empty;

        var anomaly = await _anomalyDetector.EvaluateAsync(
            tenant,
            payload.Length,
            DateTimeOffset.UtcNow,
            classification,
            context.RequestAborted);

        context.Items[GatewayHttpContextKeys.Anomaly] = anomaly;
        await next(context);
    }
}
