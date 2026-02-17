using Gateway.Core.Abstractions;
using Gateway.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Gateway.Infrastructure.Services;

public sealed class HeaderTenantResolver : ITenantResolver
{
    public ValueTask<TenantContext?> ResolveAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenant) || string.IsNullOrWhiteSpace(tenant))
        {
            return ValueTask.FromResult<TenantContext?>(null);
        }

        var plan = context.Request.Headers.TryGetValue("X-Tenant-Plan", out var p) ? p.ToString() : "standard";
        var model = new TenantContext(tenant.ToString(), plan, new Dictionary<string, string>());
        return ValueTask.FromResult<TenantContext?>(model);
    }
}
