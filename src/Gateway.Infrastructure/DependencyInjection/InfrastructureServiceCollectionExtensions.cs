using Gateway.Application.Abstractions;
using Gateway.Application.Services;
using Gateway.Core.Abstractions;
using Gateway.Core.Services;
using Gateway.Infrastructure.Options;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient("proxy");

        services.AddOptions<AiProviderOptions>()
            .BindConfiguration("Gateway:Ai")
            .ValidateDataAnnotations();

        services.AddSingleton<IAIClassifier, ResilientAiClassifier>();
        services.AddSingleton<IAnomalyDetector, DefaultAnomalyDetector>();
        services.AddSingleton<IRateLimiter, AdaptiveRedisLikeRateLimiter>();
        services.AddSingleton<IRedactor, RegexRedactor>();
        services.AddSingleton<IPayloadSummarizer, PayloadSummarizer>();
        services.AddSingleton<IWebhookReplayStore, InMemoryReplayStore>();
        services.AddSingleton<ITenantResolver, HeaderTenantResolver>();

        services.AddScoped<ISecurityDecisionOrchestrator, SecurityDecisionOrchestrator>();
        services.AddScoped<IApiDocumentationGenerator, EndpointDocumentationGenerator>();

        return services;
    }
}
