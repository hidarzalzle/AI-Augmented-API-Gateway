# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY AIAugmentedApiGateway.sln ./
COPY src/Gateway.Core/Gateway.Core.csproj src/Gateway.Core/
COPY src/Gateway.Application/Gateway.Application.csproj src/Gateway.Application/
COPY src/Gateway.Infrastructure/Gateway.Infrastructure.csproj src/Gateway.Infrastructure/
COPY src/Gateway.Api/Gateway.Api.csproj src/Gateway.Api/

RUN dotnet restore src/Gateway.Api/Gateway.Api.csproj

# Copy full source and publish
COPY src ./src
RUN dotnet publish src/Gateway.Api/Gateway.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security hardening defaults
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
