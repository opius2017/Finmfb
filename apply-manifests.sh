#!/bin/bash

# FinMFB Banking System - Kubernetes Manifest Creation Script
# This script creates Kubernetes manifests for manual deployment

echo "===== FinMFB Banking System - Kubernetes Manifest Creation ====="

# Create namespace
kubectl apply -f k8s/namespace-and-secrets.yaml

# Create secrets
kubectl apply -f k8s/secrets.yaml

# Create ConfigMap
kubectl apply -f k8s/api-configmap.yaml

# Deploy StatefulSets
kubectl apply -f k8s/database-statefulset.yaml
kubectl apply -f k8s/redis-statefulset.yaml
kubectl apply -f k8s/rabbitmq-statefulset.yaml

# Deploy API and Frontend
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml
kubectl apply -f k8s/frontend-deployment.yaml
kubectl apply -f k8s/frontend-service.yaml

# Apply horizontal pod autoscaler
kubectl apply -f k8s/api-hpa.yaml

# Apply network policies
kubectl apply -f k8s/network-policy.yaml

# Apply resource limits
kubectl apply -f k8s/resource-limits.yaml

# Apply pod disruption budgets
kubectl apply -f k8s/pod-disruption-budget.yaml

# Apply ingress
kubectl apply -f k8s/ingress.yaml

# Create monitoring namespace
kubectl apply -f k8s/monitoring-namespace.yaml

# Deploy monitoring stack
kubectl apply -f k8s/prometheus.yaml
kubectl apply -f k8s/grafana.yaml

echo "===== Kubernetes Manifests Applied ====="
echo ""
echo "Checking pod status..."
kubectl get pods -n finmfb
kubectl get pods -n finmfb-monitoring

echo ""
echo "Checking services..."
kubectl get services -n finmfb
kubectl get services -n finmfb-monitoring

echo ""
echo "Checking ingress..."
kubectl get ingress -n finmfb