using Gateway.Application.Context;
using Gateway.Core.Abstractions;

namespace Gateway.Api.Middleware;

public sealed class TenantResolutionMiddleware : IMiddleware
{
    private readonly ITenantResolver _tenantResolver;

    public TenantResolutionMiddleware(ITenantResolver tenantResolver)
    {
        _tenantResolver = tenantResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenant = await _tenantResolver.ResolveAsync(context, context.RequestAborted);
        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing tenant context.");
            return;
        }

        context.Items[GatewayHttpContextKeys.Tenant] = tenant;
        await next(context);
    }
}
