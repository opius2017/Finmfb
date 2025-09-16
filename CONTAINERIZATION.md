# FinMFB Banking System - Containerization & DevOps

This document describes the containerization and deployment strategy for the FinMFB Banking System.

## Docker & Kubernetes Infrastructure

The FinMFB Banking System is containerized using Docker and orchestrated with Kubernetes for production deployments. This architecture provides scalability, resilience, and portability across different environments.

### Container Architecture

![Container Architecture](docs/container-architecture.png)

The containerized architecture includes:

- **Backend API**: .NET 9.0 API with Clean Architecture
- **Frontend**: React with TypeScript and Tailwind CSS
- **API Gateway**: Request routing and API management
- **Databases**: SQL Server for relational data, Redis for caching
- **Message Broker**: RabbitMQ for event-driven communication
- **Monitoring Stack**: Prometheus, Grafana, and Seq for comprehensive observability

## Getting Started with Docker

### Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for local development)
- Node.js 20+ (for local development)

### Local Development with Docker Compose

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/finmfb.git
   cd finmfb
   ```

2. Run with Docker Compose
   ```bash
   docker-compose up -d
   ```

3. Access the application
   - Frontend: http://localhost
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - API Gateway: http://localhost:8080
   - Seq (Logging): http://localhost:5341
   - Prometheus: http://localhost:9090
   - Grafana: http://localhost:3000

### Understanding the Docker Compose File

The `docker-compose.yml` file defines the following services:

- **db**: SQL Server database
- **redis**: Redis for distributed caching
- **rabbitmq**: RabbitMQ for messaging
- **api**: Backend API service
- **api-gateway**: API Gateway for routing requests
- **frontend**: React frontend application
- **seq**: Centralized logging service
- **prometheus**: Metrics collection
- **grafana**: Visualization dashboards

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (AKS, EKS, GKE, or on-premises)
- kubectl and helm installed
- Container registry access

### Deployment Process

1. Update the registry in the deployment script
   ```bash
   vi deploy.sh
   # Update DOCKER_REGISTRY="your-registry.azurecr.io"
   ```

2. Run the deployment script
   ```bash
   ./deploy.sh dev  # Deploy to development environment
   ./deploy.sh prod  # Deploy to production environment
   ```

### Kubernetes Resources

The Kubernetes deployment includes:

- **Deployments**: Manage the desired state of pods
- **Services**: Expose pods to network traffic
- **Ingress**: Route external traffic to services
- **ConfigMaps**: Store configuration data
- **Secrets**: Store sensitive information
- **HorizontalPodAutoscalers**: Automatically scale pods
- **NetworkPolicies**: Control pod-to-pod communication

### Helm Chart

The Helm chart in `helm/finmfb` provides a templated approach to deploying the application with different configurations for different environments.

```bash
# Install the Helm chart
helm install finmfb ./helm/finmfb --namespace finmfb -f ./helm/finmfb/values-dev.yaml
```

## CI/CD Pipeline

The project includes a GitHub Actions workflow for continuous integration and deployment:

1. **Build**: Compile the application and run tests
2. **Security Scan**: Check for vulnerabilities
3. **Docker Build**: Create container images
4. **Deploy**: Deploy to Kubernetes using Helm

## Monitoring & Observability

The containerized application includes a comprehensive monitoring stack:

- **Prometheus**: Metrics collection
- **Grafana**: Visualization dashboards
- **Seq**: Centralized logging
- **Health Checks**: Kubernetes readiness and liveness probes

## Security Considerations

The containerized deployment includes several security features:

- **Network Policies**: Restrict pod-to-pod communication
- **Secret Management**: Secure storage of credentials
- **Image Security**: Multi-stage builds with minimal attack surface
- **Resource Limits**: Prevent resource exhaustion attacks
- **Readiness and Liveness Probes**: Ensure container health

## Scaling Strategies

The application can scale horizontally and vertically:

- **Horizontal Pod Autoscaling**: Automatically adjust pod count
- **Vertical Pod Autoscaling**: Adjust resource allocation
- **Database Scaling**: Implement read replicas or sharding
- **Caching Strategies**: Use Redis for distributed caching

## Troubleshooting

Common troubleshooting steps for the containerized application:

1. Check pod status:
   ```bash
   kubectl get pods -n finmfb
   ```

2. View pod logs:
   ```bash
   kubectl logs deployment/finmfb-api -n finmfb
   ```

3. Execute commands in containers:
   ```bash
   kubectl exec -it deployment/finmfb-api -n finmfb -- /bin/bash
   ```

4. Check service endpoints:
   ```bash
   kubectl get endpoints -n finmfb
   ```

## Contributing

Please refer to the main README for contribution guidelines.

## License

This project is licensed under the MIT License - see the LICENSE file for details.