using Gateway.Application.Abstractions;
using Gateway.Core.Abstractions;
using Gateway.Core.Models;

namespace Gateway.Application.Services;

public sealed class EndpointDocumentationGenerator : IApiDocumentationGenerator
{
    private readonly IAIClassifier _classifier;

    public EndpointDocumentationGenerator(IAIClassifier classifier)
    {
        _classifier = classifier;
    }

    public async Task<string> GenerateAsync(string endpointDefinition, CancellationToken cancellationToken)
    {
        var response = await _classifier.ClassifyAsync(
            new TenantContext("internal", "platform", new Dictionary<string, string>()),
            "api-docs",
            endpointDefinition,
            cancellationToken);

        return $"version={response.ModelVersion}; category={response.Category}; confidence={response.Confidence:0.00}";
    }
}
