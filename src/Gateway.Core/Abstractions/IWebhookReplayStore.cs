namespace Gateway.Core.Abstractions;

public interface IWebhookReplayStore
{
    Task<bool> TryRegisterAsync(string tenantId, string replayId, TimeSpan ttl, CancellationToken cancellationToken);
}
