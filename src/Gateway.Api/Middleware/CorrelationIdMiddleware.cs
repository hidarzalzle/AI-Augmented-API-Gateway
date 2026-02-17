using Gateway.Application.Context;

namespace Gateway.Api.Middleware;

public sealed class CorrelationIdMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[GatewayHttpContextKeys.CorrelationId] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        await next(context);
    }
}
