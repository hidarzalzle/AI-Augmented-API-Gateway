using Gateway.Application.Context;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;

namespace Gateway.Api.Middleware;

public sealed class RateLimitingMiddleware : IMiddleware
{
    private readonly IRateLimiter _rateLimiter;

    public RateLimitingMiddleware(IRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenant = (TenantContext)context.Items[GatewayHttpContextKeys.Tenant]!;
        var anomalyScore = context.Items.TryGetValue(GatewayHttpContextKeys.Anomaly, out var anomalyObj)
            ? ((Gateway.Core.Models.AnomalyEvaluation)anomalyObj!).Score
            : 0;

        var decision = await _rateLimiter.EvaluateAsync(tenant, context.Request.Path, anomalyScore, context.RequestAborted);
        if (!decision.Allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = Math.Ceiling(decision.RetryAfter.TotalSeconds).ToString();
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        await next(context);
    }
}
