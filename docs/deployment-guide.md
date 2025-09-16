# FinMFB Banking System Deployment Guide

This guide provides detailed instructions for deploying the FinMFB Banking System in different environments.

## Deployment Options

The FinMFB Banking System can be deployed in the following ways:

1. **Docker Compose**: For local development and testing
2. **Kubernetes**: For production and staging environments
3. **Manual Deployment**: For custom environments

## Prerequisites

### Common Prerequisites

- Git for version control
- Docker and Docker Compose for containerization
- Access to a container registry (e.g., Docker Hub, Azure Container Registry, etc.)

### Kubernetes Deployment Prerequisites

- Kubernetes cluster (AKS, EKS, GKE, or on-premises)
- kubectl command-line tool
- Helm package manager
- Access to a container registry
- Domain name and TLS certificates

## Docker Compose Deployment

Docker Compose is ideal for local development and testing. It provides a simple way to run all the required services without setting up a Kubernetes cluster.

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/finmfb.git
   cd finmfb
   ```

2. Run the setup script:
   ```bash
   ./setup-local-env.sh
   ```

3. Or manually start the services:
   ```bash
   docker-compose up -d
   ```

### Accessing the Application

- Frontend: http://localhost
- API: http://localhost:5000
- API Swagger: http://localhost:5000/swagger
- API Gateway: http://localhost:8080
- Seq (Logging): http://localhost:5341
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
- RabbitMQ Management: http://localhost:15672

## Kubernetes Deployment

Kubernetes is recommended for production and staging environments. It provides scalability, resilience, and automated management of the application.

### Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                             Kubernetes Cluster                          │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                         finmfb Namespace                        │    │
│  │                                                                 │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────┐ │    │
│  │  │  Frontend   │  │     API     │  │   Gateway   │  │  Others │ │    │
│  │  │ Deployment  │  │ Deployment  │  │ Deployment  │  │         │ │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────┘ │    │
│  │                                                                 │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │    │
│  │  │    Ingress  │  │  Services   │  │ ConfigMaps/ │             │    │
│  │  │  Controller │  │             │  │   Secrets   │             │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                      monitoring Namespace                       │    │
│  │                                                                 │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │    │
│  │  │ Prometheus  │  │   Grafana   │  │     Seq     │             │    │
│  │  │             │  │             │  │             │             │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Setup

1. Build and push Docker images:
   ```bash
   docker build -t your-registry.com/finmfb-api:latest -f Fin-Backend/Dockerfile .
   docker build -t your-registry.com/finmfb-frontend:latest -f Fin-Frontend/Dockerfile .
   
   docker push your-registry.com/finmfb-api:latest
   docker push your-registry.com/finmfb-frontend:latest
   ```

2. Create Kubernetes namespace:
   ```bash
   kubectl create namespace finmfb
   ```

3. Deploy using Helm:
   ```bash
   # For development environment
   helm install finmfb ./helm/finmfb \
     --namespace finmfb \
     -f ./helm/finmfb/values-dev.yaml \
     --set global.imageRegistry=your-registry.com
   
   # For production environment
   helm install finmfb ./helm/finmfb \
     --namespace finmfb \
     -f ./helm/finmfb/values-prod.yaml \
     --set global.imageRegistry=your-registry.com
   ```

4. Or use the deployment script:
   ```bash
   # Update the registry in the script first
   vi deploy.sh
   # Change DOCKER_REGISTRY="your-registry.com"
   
   # Deploy to development environment
   ./deploy.sh dev
   
   # Deploy to production environment
   ./deploy.sh prod
   ```

### Accessing the Application

The application can be accessed using the configured Ingress hostnames:

- Development:
  - Frontend: https://dev-portal.finmfb.com
  - API: https://dev-api.finmfb.com

- Production:
  - Frontend: https://portal.finmfb.com
  - API: https://api.finmfb.com

## Deployment Customization

### Environment Variables

The application can be customized using environment variables. These can be set in the `values.yaml` file for Helm or directly in the Kubernetes manifests.

Key environment variables:

```yaml
# Database Configuration
ConnectionStrings__DefaultConnection: "Server=db;Database=FinTechDB;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;"

# Redis Configuration
Redis__ConnectionString: "redis:6379,password=RedisStrongPassword123!"

# RabbitMQ Configuration
RabbitMQ__ConnectionString: "amqp://admin:RabbitMQStrongPassword123!@rabbitmq:5672/"

# Application Insights
ApplicationInsights__ConnectionString: "InstrumentationKey=a81fbd77-fdaa-4b3a-b7d7-4cb79df58b24;EndpointSuffix=monitor.azure.com"

# Application Settings
ASPNETCORE_ENVIRONMENT: "Production"
```

### Resource Limits

You can customize resource requests and limits in the `values.yaml` file:

```yaml
api:
  resources:
    limits:
      cpu: 1
      memory: 1Gi
    requests:
      cpu: 500m
      memory: 512Mi

frontend:
  resources:
    limits:
      cpu: 500m
      memory: 512Mi
    requests:
      cpu: 200m
      memory: 256Mi
```

### Autoscaling

You can configure autoscaling in the `values.yaml` file:

```yaml
api:
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10
    targetCPUUtilizationPercentage: 70
    targetMemoryUtilizationPercentage: 80
```

## Infrastructure Recommendations

### Production Environment

For a production environment, we recommend:

1. **Managed Kubernetes Service**:
   - Azure Kubernetes Service (AKS)
   - Amazon Elastic Kubernetes Service (EKS)
   - Google Kubernetes Engine (GKE)

2. **Managed Database**:
   - Azure SQL Database
   - Amazon RDS for SQL Server
   - Google Cloud SQL for SQL Server

3. **Managed Redis Cache**:
   - Azure Cache for Redis
   - Amazon ElastiCache for Redis
   - Google Cloud Memorystore for Redis

4. **Managed RabbitMQ**:
   - Azure Service Bus
   - Amazon MQ
   - CloudAMQP

5. **Container Registry**:
   - Azure Container Registry
   - Amazon Elastic Container Registry
   - Google Container Registry

6. **Load Balancer**:
   - Azure Application Gateway
   - AWS Application Load Balancer
   - Google Cloud Load Balancing

7. **SSL/TLS Certificates**:
   - Let's Encrypt with cert-manager
   - Managed certificates from cloud provider

## Maintenance and Operations

### Monitoring

The application includes a comprehensive monitoring stack:

1. **Prometheus**: Metrics collection
2. **Grafana**: Visualization dashboards
3. **Seq**: Centralized logging

Access the monitoring tools:

```bash
# Port forward Prometheus
kubectl port-forward svc/prometheus 9090:9090 -n finmfb

# Port forward Grafana
kubectl port-forward svc/grafana 3000:3000 -n finmfb

# Port forward Seq
kubectl port-forward svc/seq 5341:80 -n finmfb
```

### Backup and Recovery

Implement regular backups for:

1. **Database**:
   - Use managed backup solutions from cloud providers
   - Set up SQL Server scheduled backups

2. **Configuration**:
   - Backup Kubernetes manifests and Helm values
   - Use GitOps for configuration management

3. **Application Data**:
   - Backup persistent volumes
   - Use volume snapshots

### Upgrading

To upgrade the application:

1. Update the Docker images:
   ```bash
   docker build -t your-registry.com/finmfb-api:new-version -f Fin-Backend/Dockerfile .
   docker build -t your-registry.com/finmfb-frontend:new-version -f Fin-Frontend/Dockerfile .
   
   docker push your-registry.com/finmfb-api:new-version
   docker push your-registry.com/finmfb-frontend:new-version
   ```

2. Upgrade the Helm release:
   ```bash
   helm upgrade finmfb ./helm/finmfb \
     --namespace finmfb \
     -f ./helm/finmfb/values-prod.yaml \
     --set global.imageRegistry=your-registry.com \
     --set api.image.tag=new-version \
     --set frontend.image.tag=new-version
   ```

3. Or use the deployment script with a tag:
   ```bash
   TAG=new-version ./deploy.sh prod
   ```

## Troubleshooting

For detailed troubleshooting guidance, see the [Kubernetes Troubleshooting Guide](kubernetes-troubleshooting.md).

Common troubleshooting commands:

```bash
# Check pod status
kubectl get pods -n finmfb

# Check pod logs
kubectl logs <pod-name> -n finmfb

# Check deployment status
kubectl describe deployment <deployment-name> -n finmfb

# Check service status
kubectl describe service <service-name> -n finmfb

# Check ingress status
kubectl describe ingress <ingress-name> -n finmfb

# Check persistent volume claims
kubectl get pvc -n finmfb

# Check events
kubectl get events -n finmfb --sort-by='.lastTimestamp'
```

## Security Considerations

1. **Secrets Management**:
   - Store sensitive information in Kubernetes Secrets
   - Consider using a secrets management solution like HashiCorp Vault

2. **Network Security**:
   - Implement network policies to restrict pod-to-pod communication
   - Use TLS for all external communication
   - Implement API authentication and authorization

3. **Container Security**:
   - Use minimal base images
   - Run containers as non-root users
   - Use read-only file systems where possible
   - Implement resource limits

4. **Access Control**:
   - Implement RBAC for Kubernetes access
   - Use service accounts with minimal permissions
   - Implement audit logging

5. **Compliance**:
   - Follow industry-specific compliance requirements
   - Implement data protection measures
   - Conduct regular security audits

## Contact and Support

For support with deployment issues, please contact:

- Email: support@finmfb.com
- Phone: +1-234-567-8900
- Issue Tracker: https://github.com/yourusername/finmfb/issues