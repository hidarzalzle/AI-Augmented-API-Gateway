using System.Text;
using Gateway.Application.Context;
using Gateway.Application.Options;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;
using Microsoft.Extensions.Options;

namespace Gateway.Api.Middleware;

public sealed class PayloadInspectionMiddleware : IMiddleware
{
    private readonly IRedactor _redactor;
    private readonly IPayloadSummarizer _summarizer;
    private readonly GatewayOptions _options;

    public PayloadInspectionMiddleware(IRedactor redactor, IPayloadSummarizer summarizer, IOptions<GatewayOptions> options)
    {
        _redactor = redactor;
        _summarizer = summarizer;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        if (Encoding.UTF8.GetByteCount(payload) > _options.MaxRequestBytes)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsync("Payload too large.");
            return;
        }

        var redacted = _redactor.Redact(payload);
        context.Items[GatewayHttpContextKeys.RedactedPayload] = redacted;

        var tenant = (TenantContext)context.Items[GatewayHttpContextKeys.Tenant]!;
        var summary = await _summarizer.SummarizeAsync(tenant, redacted, _options.SummarizationThresholdBytes, context.RequestAborted);
        context.Items[GatewayHttpContextKeys.PayloadSummary] = summary;

        await next(context);
    }
}
