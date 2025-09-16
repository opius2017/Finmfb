#!/bin/bash
set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Print with color
print_green() { echo -e "${GREEN}$1${NC}"; }
print_yellow() { echo -e "${YELLOW}$1${NC}"; }
print_red() { echo -e "${RED}$1${NC}"; }

# Configuration Variables
DOCKER_REGISTRY="your-registry.azurecr.io"
NAMESPACE="finmfb"
TAG=$(git describe --tags --always)
ENV=${1:-dev} # Default to dev if not specified

print_yellow "Deploying FinMFB Banking Application to $ENV environment..."

# Check if kubectl is installed
if ! command -v kubectl &> /dev/null; then
    print_red "kubectl is not installed. Please install kubectl and try again."
    exit 1
fi

# Check if helm is installed
if ! command -v helm &> /dev/null; then
    print_red "helm is not installed. Please install helm and try again."
    exit 1
fi

# Check if docker is installed
if ! command -v docker &> /dev/null; then
    print_red "docker is not installed. Please install docker and try again."
    exit 1
fi

# Build and push Docker images
build_and_push_images() {
    print_yellow "Building and pushing Docker images..."
    
    # Build and push backend API
    print_yellow "Building backend API image..."
    docker build -t $DOCKER_REGISTRY/finmfb-api:$TAG -f Fin-Backend/Dockerfile .
    docker push $DOCKER_REGISTRY/finmfb-api:$TAG
    
    # Build and push frontend
    print_yellow "Building frontend image..."
    docker build -t $DOCKER_REGISTRY/finmfb-frontend:$TAG -f Fin-Frontend/Dockerfile .
    docker push $DOCKER_REGISTRY/finmfb-frontend:$TAG
    
    print_green "All images built and pushed successfully!"
}

# Create namespace if it doesn't exist
create_namespace() {
    print_yellow "Creating namespace $NAMESPACE if it doesn't exist..."
    kubectl get namespace $NAMESPACE > /dev/null 2>&1 || kubectl create namespace $NAMESPACE
    print_green "Namespace ready!"
}

# Deploy application using Helm
deploy_app() {
    print_yellow "Deploying application to $ENV environment..."
    
    # Set values file based on environment
    VALUES_FILE="helm/finmfb/values-${ENV}.yaml"
    if [ ! -f "$VALUES_FILE" ]; then
        print_red "Values file $VALUES_FILE not found. Using default values."
        VALUES_FILE="helm/finmfb/values.yaml"
    fi
    
    # Deploy or upgrade with Helm
    if helm status finmfb -n $NAMESPACE > /dev/null 2>&1; then
        print_yellow "Upgrading existing deployment..."
        helm upgrade finmfb helm/finmfb \
            --namespace $NAMESPACE \
            -f $VALUES_FILE \
            --set global.imageRegistry=$DOCKER_REGISTRY \
            --set api.image.tag=$TAG \
            --set frontend.image.tag=$TAG
    else
        print_yellow "Creating new deployment..."
        helm install finmfb helm/finmfb \
            --namespace $NAMESPACE \
            -f $VALUES_FILE \
            --set global.imageRegistry=$DOCKER_REGISTRY \
            --set api.image.tag=$TAG \
            --set frontend.image.tag=$TAG
    fi
    
    print_green "Deployment completed successfully!"
}

# Check deployment status
check_deployment() {
    print_yellow "Checking deployment status..."
    kubectl get pods -n $NAMESPACE
    kubectl get svc -n $NAMESPACE
    kubectl get ingress -n $NAMESPACE
    print_green "Deployment status check completed!"
}

# Main execution
build_and_push_images
create_namespace
deploy_app
check_deployment

print_green "FinMFB Banking Application deployed successfully to $ENV environment!"
print_green "Access the application at:"
kubectl get ingress -n $NAMESPACE -o jsonpath="{.items[0].spec.rules[*].host}" | tr ' ' '\n' | xargs -I {} echo "https://{}"