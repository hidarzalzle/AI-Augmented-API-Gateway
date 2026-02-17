using Gateway.Api.Middleware;

namespace Gateway.Api.Extensions;

public static class MiddlewareRegistrationExtensions
{
    public static IServiceCollection AddGatewayMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<CorrelationIdMiddleware>();
        services.AddTransient<TenantResolutionMiddleware>();
        services.AddTransient<RateLimitingMiddleware>();
        services.AddTransient<PayloadInspectionMiddleware>();
        services.AddTransient<AiClassificationMiddleware>();
        services.AddTransient<AnomalyDetectionMiddleware>();
        services.AddTransient<ProxyForwardingMiddleware>();
        services.AddTransient<ObservabilityMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseGatewayPipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseMiddleware<PayloadInspectionMiddleware>();
        app.UseMiddleware<AiClassificationMiddleware>();
        app.UseMiddleware<AnomalyDetectionMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<ProxyForwardingMiddleware>();
        app.UseMiddleware<ObservabilityMiddleware>();
        return app;
    }
}
