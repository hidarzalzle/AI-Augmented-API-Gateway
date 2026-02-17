using Gateway.Application.Context;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;

namespace Gateway.Api.Middleware;

public sealed class AiClassificationMiddleware : IMiddleware
{
    private readonly IAIClassifier _classifier;

    public AiClassificationMiddleware(IAIClassifier classifier)
    {
        _classifier = classifier;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenant = (TenantContext)context.Items[GatewayHttpContextKeys.Tenant]!;
        var payload = context.Items[GatewayHttpContextKeys.RedactedPayload]?.ToString() ?? string.Empty;
        var fingerprint = context.Items[GatewayHttpContextKeys.CorrelationId]?.ToString() ?? Guid.NewGuid().ToString("N");

        var result = await _classifier.ClassifyAsync(tenant, fingerprint, payload, context.RequestAborted);
        context.Items[GatewayHttpContextKeys.Classification] = result;

        await next(context);
    }
}
