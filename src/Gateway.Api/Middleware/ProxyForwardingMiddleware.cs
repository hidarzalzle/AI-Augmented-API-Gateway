using System.Net.Http.Headers;
using System.Text;
using Gateway.Application.Context;
using Gateway.Application.Options;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;
using Microsoft.Extensions.Options;

namespace Gateway.Api.Middleware;

public sealed class ProxyForwardingMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GatewayOptions _options;
    private readonly IWebhookReplayStore _replayStore;

    public ProxyForwardingMiddleware(IHttpClientFactory httpClientFactory, IOptions<GatewayOptions> options, IWebhookReplayStore replayStore)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _replayStore = replayStore;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/proxy"))
        {
            await next(context);
            return;
        }

        var tenant = (TenantContext)context.Items[GatewayHttpContextKeys.Tenant]!;
        if (context.Request.Path.StartsWithSegments("/proxy/webhook"))
        {
            if (!context.Request.Headers.TryGetValue("X-Webhook-Signature", out _))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing webhook signature.");
                return;
            }

            var replayId = context.Request.Headers["X-Webhook-Id"].ToString();
            var ok = await _replayStore.TryRegisterAsync(tenant.TenantId, replayId, TimeSpan.FromMinutes(5), context.RequestAborted);
            if (!ok)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsync("Replay detected.");
                return;
            }
        }

        var downstreamUrl = _options.DownstreamBaseUrl.TrimEnd('/') + context.Request.Path + context.Request.QueryString;
        var body = context.Items[GatewayHttpContextKeys.RedactedPayload]?.ToString() ?? string.Empty;

        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), downstreamUrl)
        {
            Content = new StringContent(body, Encoding.UTF8)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var client = _httpClientFactory.CreateClient("proxy");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        context.Response.StatusCode = (int)response.StatusCode;

        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        var responseBody = await response.Content.ReadAsStringAsync(context.RequestAborted);
        await context.Response.WriteAsync(responseBody);
    }
}
