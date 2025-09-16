# FinMFB Helm Chart

This Helm chart deploys the FinMFB Banking Application in a Kubernetes cluster.

## Prerequisites

- Kubernetes 1.19+
- Helm 3.2.0+
- Ingress controller (e.g., NGINX Ingress Controller)
- Cert Manager (for TLS certificates)
- Prometheus and Grafana (for monitoring)

## Installation

### Add the Helm repository

```bash
helm repo add finmfb https://helm.finmfb.com
helm repo update
```

### Install the chart

```bash
# Create namespace
kubectl create namespace finmfb

# Install with default values
helm install finmfb finmfb/finmfb --namespace finmfb

# Install with custom values
helm install finmfb finmfb/finmfb --namespace finmfb -f values.yaml
```

## Configuration

The following table lists the configurable parameters of the FinMFB chart and their default values.

| Parameter | Description | Default |
|-----------|-------------|---------|
| `global.environment` | Environment name | `production` |
| `global.imageRegistry` | Global Docker image registry | `""` |
| `global.imagePullSecrets` | Global Docker registry secret names as an array | `[]` |
| `api.replicaCount` | Number of API replicas | `3` |
| `api.image.repository` | API image repository | `finmfb-api` |
| `api.image.tag` | API image tag | `latest` |
| `api.image.pullPolicy` | API image pull policy | `Always` |
| `api.resources` | API resource requests and limits | See `values.yaml` |
| `api.autoscaling.enabled` | Enable autoscaling for API | `true` |
| `api.autoscaling.minReplicas` | Minimum replicas for API | `3` |
| `api.autoscaling.maxReplicas` | Maximum replicas for API | `10` |
| `api.autoscaling.targetCPUUtilizationPercentage` | Target CPU utilization percentage | `70` |
| `api.autoscaling.targetMemoryUtilizationPercentage` | Target memory utilization percentage | `80` |
| `frontend.replicaCount` | Number of Frontend replicas | `3` |
| `frontend.image.repository` | Frontend image repository | `finmfb-frontend` |
| `frontend.image.tag` | Frontend image tag | `latest` |
| `frontend.image.pullPolicy` | Frontend image pull policy | `Always` |
| `frontend.resources` | Frontend resource requests and limits | See `values.yaml` |
| `frontend.autoscaling.enabled` | Enable autoscaling for Frontend | `true` |
| `ingress.enabled` | Enable ingress | `true` |
| `ingress.className` | Ingress class name | `nginx` |
| `ingress.annotations` | Ingress annotations | See `values.yaml` |
| `ingress.hosts` | Ingress hostnames | See `values.yaml` |
| `ingress.tls` | Ingress TLS configuration | See `values.yaml` |
| `secrets.create` | Create secrets | `true` |
| `secrets.dbConnectionString` | Database connection string | `""` |
| `secrets.redisConnectionString` | Redis connection string | `""` |
| `secrets.rabbitmqConnectionString` | RabbitMQ connection string | `""` |
| `secrets.appInsightsConnectionString` | Application Insights connection string | `""` |
| `networkPolicy.enabled` | Enable network policy | `true` |

## Uninstalling the Chart

To uninstall/delete the `finmfb` deployment:

```bash
helm delete finmfb --namespace finmfb
```

## Upgrading the Chart

To upgrade the `finmfb` deployment:

```bash
helm upgrade finmfb finmfb/finmfb --namespace finmfb
```