using Gateway.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Gateway.Core.Abstractions;

public interface ITenantResolver
{
    ValueTask<TenantContext?> ResolveAsync(HttpContext context, CancellationToken cancellationToken);
}
