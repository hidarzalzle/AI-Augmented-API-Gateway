using Gateway.Api.Extensions;
using Gateway.Application.Options;
using Gateway.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GatewayOptions>(builder.Configuration.GetSection("Gateway"));
builder.Services.AddGatewayInfrastructure();
builder.Services.AddGatewayMiddlewares();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "ai-augmented-api-gateway" }));
app.MapGet("/docs/version", () => Results.Ok(new { docsVersion = "v1" }));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI-Augmented API Gateway v1");
    options.RoutePrefix = "swagger";
});

app.UseGatewayPipeline();

app.Run();
