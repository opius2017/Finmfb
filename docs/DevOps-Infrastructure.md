# DevOps Infrastructure Documentation

This document provides a comprehensive overview of the DevOps infrastructure implemented for the FinMFB Banking Application.

## Table of Contents

1. [Infrastructure as Code (Terraform)](#infrastructure-as-code-terraform)
2. [Containerization (Docker)](#containerization-docker)
3. [Container Orchestration (Kubernetes)](#container-orchestration-kubernetes)
4. [API Gateway](#api-gateway)
5. [Circuit Breakers & Resilience Patterns](#circuit-breakers--resilience-patterns)
6. [Distributed Caching](#distributed-caching)
7. [Monitoring & Observability](#monitoring--observability)
8. [CI/CD Pipeline](#cicd-pipeline)

## Infrastructure as Code (Terraform)

The FinMFB infrastructure is managed using Terraform to provision and maintain cloud resources in a consistent and repeatable manner.

### Directory Structure

```
terraform/
├── modules/                  # Reusable Terraform modules
│   ├── acr/                  # Azure Container Registry
│   ├── aks/                  # Azure Kubernetes Service
│   ├── app_gateway/          # Application Gateway (API Gateway)
│   ├── database/             # Azure Database for PostgreSQL
│   ├── key_vault/            # Azure Key Vault
│   ├── monitoring/           # Log Analytics and Application Insights
│   ├── network/              # Virtual Network and Subnets
│   └── redis/                # Azure Redis Cache
└── environments/             # Environment-specific configurations
    ├── dev/                  # Development environment
    └── prod/                 # Production environment
```

### Key Components

- **Resource Groups**: Logical containers for Azure resources
- **Networking**: VNets, subnets, and NSGs for secure communication
- **Kubernetes (AKS)**: Managed Kubernetes service for container orchestration
- **Container Registry (ACR)**: Storage for Docker images
- **Database**: Managed PostgreSQL service
- **Redis Cache**: Managed Redis service for distributed caching
- **Application Gateway**: API Gateway for routing and security
- **Key Vault**: Secure storage for secrets and certificates
- **Monitoring**: Log Analytics and Application Insights for observability

### Environment-Specific Configurations

Each environment (dev, prod) has its own Terraform configuration with appropriate settings:

- **Development**: Lower resource specifications, minimal redundancy
- **Production**: High availability, auto-scaling, geo-redundancy

## Containerization (Docker)

The application is containerized using Docker with multi-stage builds to optimize container size and security.

### Docker Images

- **Backend API**: .NET Core application with minimal runtime
- **Frontend**: React application served through Nginx
- **Database**: PostgreSQL for development (managed service in production)
- **Redis**: Redis for caching (managed service in production)

### Multi-Stage Build Example

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Fin-Backend/FinTech.WebAPI.csproj", "Fin-Backend/"]
RUN dotnet restore "Fin-Backend/FinTech.WebAPI.csproj"
COPY . .
WORKDIR "/src/Fin-Backend"
RUN dotnet build "FinTech.WebAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FinTech.WebAPI.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinTech.WebAPI.dll"]
```

## Container Orchestration (Kubernetes)

Kubernetes is used for container orchestration to ensure high availability, scalability, and resilience.

### Kubernetes Resources

- **Deployments**: Manages the desired state of pods
- **Services**: Exposes pods as network services
- **ConfigMaps**: Configuration data separate from code
- **Secrets**: Sensitive configuration data
- **Ingress**: External access to services
- **StatefulSets**: For stateful workloads like databases
- **HorizontalPodAutoscalers**: Automatic scaling based on CPU/memory
- **NetworkPolicies**: Control traffic between pods

### Helm Charts

Helm is used for packaging Kubernetes applications with environment-specific configurations:

```
helm/
├── finmfb/
│   ├── Chart.yaml              # Chart metadata
│   ├── values.yaml             # Default values
│   ├── values-dev.yaml         # Development values
│   ├── values-prod.yaml        # Production values
│   └── templates/              # Kubernetes manifest templates
│       ├── api-deployment.yaml
│       ├── api-service.yaml
│       ├── frontend-deployment.yaml
│       ├── frontend-service.yaml
│       ├── ingress.yaml
│       └── ...
```

## API Gateway

Application Gateway is used as an API Gateway to provide a unified entry point for the microservices.

### Features

- **Routing**: Route requests to appropriate microservices
- **Load Balancing**: Distribute traffic across multiple instances
- **SSL Termination**: Handle HTTPS traffic
- **Web Application Firewall (WAF)**: Protect against common web vulnerabilities
- **Authentication**: JWT validation and OAuth 2.0 support
- **Rate Limiting**: Prevent abuse of APIs

### Terraform Configuration

The Application Gateway is configured using Terraform with:

- **Backend Pools**: Define the backend services
- **HTTP Settings**: Configure connection settings
- **Listeners**: Configure frontend ports and protocols
- **Rules**: Define routing rules
- **WAF Configuration**: Security rules and policies

## Circuit Breakers & Resilience Patterns

Circuit breakers are implemented using Polly to prevent cascading failures and provide resilience.

### Implementation

The `ResiliencePolicies.cs` class defines various resilience policies:

```csharp
// Retry policy with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// Circuit breaker policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Timeout policy
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

// Bulkhead policy
var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(20, 10);
```

### Integration

These policies are registered in the DI container and applied to HTTP clients:

```csharp
services.AddHttpClient("banking-integration")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .AddPolicyHandler(timeoutPolicy)
    .AddPolicyHandler(bulkheadPolicy);
```

## Distributed Caching

Redis is used for distributed caching to improve performance and reduce database load.

### Implementation

The `RedisDistributedCacheService.cs` class provides methods for cache operations:

```csharp
// Get from cache
public async Task<T> GetAsync<T>(string key) where T : class
{
    var cachedValue = await _distributedCache.GetStringAsync(key);
    return JsonConvert.DeserializeObject<T>(cachedValue);
}

// Set to cache
public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
{
    var options = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = expiration
    };
    
    await _distributedCache.SetStringAsync(
        key,
        JsonConvert.SerializeObject(value),
        options);
}
```

### Cache Invalidation

Cache entries are invalidated after data changes to ensure consistency:

```csharp
// Invalidate cache after transaction
await _cacheService.RemoveAsync($"account:{transaction.FromAccount}");
if (!string.IsNullOrEmpty(transaction.ToAccount))
{
    await _cacheService.RemoveAsync($"account:{transaction.ToAccount}");
}
```

## Monitoring & Observability

A comprehensive monitoring stack is implemented to provide visibility into the application's health and performance.

### Components

- **Application Insights**: Application performance monitoring
- **Log Analytics**: Centralized logging and analysis
- **Prometheus**: Metrics collection and alerting
- **Grafana**: Metrics visualization and dashboards
- **Serilog**: Structured logging for the application

### Key Metrics

- **Request Rates**: Number of requests per minute
- **Error Rates**: Percentage of failed requests
- **Response Times**: Average and percentile response times
- **CPU & Memory Usage**: Resource utilization of containers
- **Database Performance**: Query execution times and connection counts
- **Cache Hit Ratios**: Effectiveness of the caching strategy

## CI/CD Pipeline

A continuous integration and deployment pipeline is implemented using GitHub Actions.

### Workflow

1. **Trigger**: Push to main branch or pull request
2. **Build**: Compile and build the application
3. **Test**: Run unit tests and integration tests
4. **Analyze**: Run code quality and security scans
5. **Publish**: Build and push Docker images
6. **Deploy**: Deploy to Kubernetes using Helm

### GitHub Actions Workflow

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build

  build-and-push-images:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    steps:
      - uses: actions/checkout@v3
      - name: Build and push API image
        uses: docker/build-push-action@v4
        with:
          context: .
          file: Fin-Backend/Dockerfile
          push: true
          tags: acr.azurecr.io/finmfb/api:latest
      - name: Build and push Frontend image
        uses: docker/build-push-action@v4
        with:
          context: ./Fin-Frontend
          push: true
          tags: acr.azurecr.io/finmfb/frontend:latest

  deploy:
    needs: build-and-push-images
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    steps:
      - uses: actions/checkout@v3
      - name: Install Helm
        uses: azure/setup-helm@v3
      - name: Login to AKS
        uses: azure/aks-set-context@v3
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
          cluster-name: finmfb-dev-aks
          resource-group: finmfb-dev-rg
      - name: Deploy to AKS
        run: |
          helm upgrade --install finmfb ./helm/finmfb \
            --values ./helm/finmfb/values-dev.yaml \
            --set image.tag=latest
```