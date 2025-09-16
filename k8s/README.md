# Kubernetes Deployment for FinMFB

This directory contains the Kubernetes manifests for deploying the FinMFB banking application across different environments.

## Environment Structure

The deployment is organized into three environments:

- **Development** (`k8s/development/`): For ongoing development and testing
- **Staging** (`k8s/staging/`): For pre-production validation and UAT
- **Production** (`k8s/production/`): For live customer-facing services

## Components

Each environment includes manifests for:

1. **Backend** (`backend.yaml`): The .NET Core backend API service
2. **Frontend** (`frontend.yaml`): The React/TypeScript frontend application
3. **Shared Infrastructure** (`shared-infrastructure.yaml`): Supporting services like Redis and RabbitMQ
4. **Secrets and Security** (`secrets-and-security.yaml`): Configuration for secrets, namespaces, and network policies

## Deployment Guidelines

### Prerequisites

- Kubernetes cluster (AKS, EKS, GKE, or similar)
- `kubectl` configured to access your cluster
- Helm (for certain dependencies)
- Container registry access (GitHub Container Registry)

### Setting Up Environments

1. **Create Namespaces**:
   ```bash
   kubectl apply -f k8s/[environment]/secrets-and-security.yaml
   ```

2. **Deploy Backend**:
   ```bash
   kubectl apply -f k8s/[environment]/backend.yaml
   ```

3. **Deploy Frontend**:
   ```bash
   kubectl apply -f k8s/[environment]/frontend.yaml
   ```

4. **Deploy Shared Infrastructure**:
   ```bash
   kubectl apply -f k8s/[environment]/shared-infrastructure.yaml
   ```

### Secret Management

⚠️ **Important**: The secrets in these manifests contain placeholder values. In a production environment:

1. **Never commit actual secrets to version control**
2. Use a secrets management solution like:
   - HashiCorp Vault
   - AWS Secrets Manager
   - Azure Key Vault
   - Kubernetes External Secrets
   - Sealed Secrets

3. Inject secrets during CI/CD deployment

### Network Policies

The deployment includes network policies to restrict traffic between components:

- Backend services can only communicate with Redis and RabbitMQ
- Frontend services can only communicate with backend services
- External access is controlled via Ingress resources

### Scaling

Horizontal Pod Autoscalers (HPAs) are configured for both frontend and backend to handle varying loads:

- **Development**: Minimal resources for development work
- **Staging**: Moderate resources for testing
- **Production**: High availability with multiple replicas and resource limits

## Monitoring & Logging

- Backend services expose Prometheus metrics at `/metrics`
- Use the monitoring stack deployed in your cluster (Prometheus, Grafana)
- Consider setting up Loki for log aggregation

## Certificate Management

TLS certificates are managed by cert-manager with Let's Encrypt:

- Development: Staging issuer (self-signed)
- Staging: Staging issuer (self-signed)
- Production: Production issuer (valid certificates)

## Troubleshooting

Common issues and solutions:

1. **Pod startup failures**: Check logs with `kubectl logs -n [namespace] [pod-name]`
2. **Connection issues**: Verify network policies and service endpoints
3. **Secret issues**: Ensure secrets are correctly created and referenced

## CI/CD Integration

These manifests are designed to be applied by the GitHub Actions workflows in `.github/workflows/`:

- `.github/workflows/backend-ci-cd.yml`: CI/CD for backend
- `.github/workflows/frontend-ci-cd.yml`: CI/CD for frontend
- `.github/workflows/main-ci-cd.yml`: Orchestrates the overall deployment process