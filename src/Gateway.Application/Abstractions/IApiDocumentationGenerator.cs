namespace Gateway.Application.Abstractions;

public interface IApiDocumentationGenerator
{
    Task<string> GenerateAsync(string endpointDefinition, CancellationToken cancellationToken);
}
