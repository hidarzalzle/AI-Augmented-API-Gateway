using System.Diagnostics;
using Gateway.Application.Context;
using Microsoft.Extensions.Logging;

namespace Gateway.Api.Middleware;

public sealed class ObservabilityMiddleware : IMiddleware
{
    private readonly ILogger<ObservabilityMiddleware> _logger;

    public ObservabilityMiddleware(ILogger<ObservabilityMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        sw.Stop();

        var correlationId = context.Items[GatewayHttpContextKeys.CorrelationId]?.ToString();
        var anomalyScore = context.Items.TryGetValue(GatewayHttpContextKeys.Anomaly, out var anomalyObj)
            ? ((Gateway.Core.Models.AnomalyEvaluation)anomalyObj!).Score
            : 0;

        _logger.LogInformation(
            "gateway.request duration_ms={Duration} status={Status} correlation={CorrelationId} anomaly={Anomaly}",
            sw.ElapsedMilliseconds,
            context.Response.StatusCode,
            correlationId,
            anomalyScore);
    }
}
