# AI-Augmented API Gateway (.NET 8)

Production-oriented, multi-tenant API Gateway with AI-assisted traffic classification, anomaly scoring, adaptive rate limiting, payload summarization, webhook replay protection, and proxy forwarding.

## Architecture

### 1) Core (`src/Gateway.Core`)
Pure domain + abstractions:
- `IAIClassifier`, `IAnomalyDetector`, `IRateLimiter`, `IPayloadSummarizer`, `ITenantResolver`
- Domain models for classification, anomaly score, tenant context, and rate-limit decisions
- Default anomaly detector logic independent of infrastructure

### 2) Application (`src/Gateway.Application`)
Use-case orchestration:
- Security decision orchestration service
- Structured event model (`SecurityDecisionEvent`)
- Shared context keys used by middleware chain
- API documentation generator abstraction

### 3) Infrastructure (`src/Gateway.Infrastructure`)
Implementations + adapters:
- Resilient AI classifier with timeout, cache, and circuit-breaker behavior
- Adaptive (Redis-like sliding-window) rate limiter with tenant + anomaly penalties
- Header tenant resolver
- Regex payload redaction
- Payload summarization with secure-reference handoff
- Webhook replay store

### 4) API (`src/Gateway.Api`)
Pipeline assembly + endpoint registration:
- Full middleware chain for correlation, tenant resolution, payload inspection, AI classification, anomaly detection, rate limiting, proxy forwarding, and observability
- Health checks + basic endpoints

## Middleware Ordering

The registered order is:
1. `CorrelationIdMiddleware`
2. `TenantResolutionMiddleware`
3. `PayloadInspectionMiddleware`
4. `AiClassificationMiddleware`
5. `AnomalyDetectionMiddleware`
6. `RateLimitingMiddleware`
7. `ProxyForwardingMiddleware`
8. `ObservabilityMiddleware`

Why this order:
- Correlation and tenant context are prerequisites for all downstream decisions.
- Payload inspection runs before AI/anomaly to enforce size limits, redaction, and summarization first.
- AI classification precedes anomaly scoring because classification is an anomaly signal.
- Rate limiting occurs after anomaly so high-risk traffic gets stricter limits.
- Proxy forwarding runs after security decisions and may short-circuit response handling.
- Observability is last to measure end-to-end latency and final status code.

## Reliability and Security Notes

- AI failures degrade to fallback classification (`Unknown`) and do not block request flow.
- AI calls are timeout-bound and circuit-breaker-protected.
- Request payloads are redacted before logging/AI handling.
- Request size is enforced (`413` for oversized payloads).
- Webhook mode validates signature header and replay IDs.
- Tenant context is required (`400` when missing).

## Project Structure

```text
src/
  Gateway.Core/
    Abstractions/
    Enums/
    Models/
    Policies/
    Services/
  Gateway.Application/
    Abstractions/
    Context/
    Events/
    Options/
    Services/
  Gateway.Infrastructure/
    DependencyInjection/
    Options/
    Services/
  Gateway.Api/
    Extensions/
    Middleware/
    Program.cs
tests/
  Gateway.Tests/
```

## Running

```bash
dotnet build
dotnet test
dotnet run --project src/Gateway.Api
```

> If downstream proxy target is not available, `/proxy/*` requests will fail fast with upstream HTTP errors.

Swagger UI is available at `/swagger` when the API is running.

## Containerization

A production-oriented multi-stage Dockerfile is available at the repository root.

Build image:

```bash
docker build -t ai-augmented-api-gateway:latest .
```

Run container:

```bash
docker run --rm -p 8080:8080 \
  -e Gateway__DownstreamBaseUrl=http://host.docker.internal:9000 \
  ai-augmented-api-gateway:latest
```

The container listens on port `8080` and starts in `Production` environment by default.
