namespace Gateway.Core.Models;

public sealed record TenantContext(
    string TenantId,
    string Plan,
    IReadOnlyDictionary<string, string> Attributes);
