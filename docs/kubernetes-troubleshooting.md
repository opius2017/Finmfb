# Kubernetes Troubleshooting Guide

This guide provides steps for troubleshooting common issues with the FinMFB Banking System in Kubernetes.

## Pod Issues

### Pod in Pending State

```bash
# Check pod status
kubectl get pod <pod-name> -n finmfb

# Check pod details
kubectl describe pod <pod-name> -n finmfb
```

Common causes:
- **Insufficient resources**: The node doesn't have enough CPU or memory
- **PVC binding issues**: PersistentVolumeClaim is not bound
- **ImagePullBackOff**: The container image cannot be pulled

### Pod in CrashLoopBackOff

```bash
# Check pod logs
kubectl logs <pod-name> -n finmfb

# Check previous container logs if container has restarted
kubectl logs <pod-name> -n finmfb --previous
```

Common causes:
- **Application errors**: Check the application logs
- **Liveness probe failures**: Check the probe configuration
- **Resource constraints**: Container might be OOM killed

### Pod Evicted

```bash
# Check node conditions
kubectl describe node <node-name>
```

Common causes:
- **Node pressure**: Node is running out of resources
- **Taint effects**: Node has NoExecute taints

## Service Issues

### Service Not Routing Traffic

```bash
# Check service details
kubectl describe service <service-name> -n finmfb

# Check endpoints
kubectl get endpoints <service-name> -n finmfb

# Test service using temporary pod
kubectl run -it --rm debug --image=busybox -- wget -O- <service-name>:<port>
```

Common causes:
- **Selector mismatch**: Service selector doesn't match pod labels
- **Port mismatch**: Service port doesn't match container port
- **Pod not ready**: Pods haven't passed readiness probes

## Ingress Issues

### Ingress Not Routing Traffic

```bash
# Check ingress
kubectl describe ingress <ingress-name> -n finmfb

# Check ingress controller logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller
```

Common causes:
- **Incorrect host**: The hostname doesn't match the request
- **Service issues**: The backend service is not available
- **TLS certificate issues**: Certificate is invalid or expired

## Network Policy Issues

### Pods Cannot Communicate

```bash
# Check network policies
kubectl get networkpolicy -n finmfb

# Describe specific policy
kubectl describe networkpolicy <policy-name> -n finmfb

# Test network connectivity with temporary pod
kubectl run -it --rm debug --image=busybox -- wget -O- <service-name>:<port>
```

Common causes:
- **Restrictive policies**: Network policy is blocking traffic
- **Missing egress rules**: Outbound traffic is blocked
- **Missing ingress rules**: Inbound traffic is blocked

## ConfigMap and Secret Issues

### Application Cannot Access ConfigMap or Secret

```bash
# Check if ConfigMap/Secret exists
kubectl get configmap <configmap-name> -n finmfb
kubectl get secret <secret-name> -n finmfb

# Check pod environment variables
kubectl exec <pod-name> -n finmfb -- env

# Check volume mounts
kubectl describe pod <pod-name> -n finmfb
```

Common causes:
- **Mounting issues**: Volume not correctly mounted
- **Reference issues**: Environment variables not set correctly
- **Permission issues**: Container cannot read the mounted files

## Persistent Volume Issues

### PVC Stuck in Pending

```bash
# Check PVC status
kubectl get pvc -n finmfb

# Describe PVC
kubectl describe pvc <pvc-name> -n finmfb

# Check available PVs
kubectl get pv
```

Common causes:
- **No matching volumes**: No PV matches the PVC requirements
- **Storage class issues**: Specified storage class doesn't exist
- **Capacity issues**: Requested capacity exceeds available capacity

## Resource Issues

### Pods Throttled or OOM Killed

```bash
# Check resource usage
kubectl top pod -n finmfb

# Check container logs
kubectl logs <pod-name> -n finmfb

# Describe pod for OOM events
kubectl describe pod <pod-name> -n finmfb
```

Common causes:
- **Insufficient limits**: Container needs more resources
- **Memory leaks**: Application has memory leaks
- **Resource spikes**: Sudden increases in resource usage

## Application-Specific Issues

### Database Connection Issues

```bash
# Check database pod status
kubectl get pod -l app=db -n finmfb

# Check database logs
kubectl logs <db-pod-name> -n finmfb

# Check API logs for connection errors
kubectl logs <api-pod-name> -n finmfb
```

Common causes:
- **Database not ready**: Database pod is not running
- **Incorrect connection string**: Environment variables misconfigured
- **Network policy**: Network policy blocking database traffic

### Redis Connection Issues

```bash
# Check Redis pod status
kubectl get pod -l app=redis -n finmfb

# Check Redis logs
kubectl logs <redis-pod-name> -n finmfb

# Test Redis connection from API pod
kubectl exec -it <api-pod-name> -n finmfb -- redis-cli -h redis ping
```

Common causes:
- **Redis not ready**: Redis pod is not running
- **Authentication issues**: Incorrect password
- **Network policy**: Network policy blocking Redis traffic

### RabbitMQ Connection Issues

```bash
# Check RabbitMQ pod status
kubectl get pod -l app=rabbitmq -n finmfb

# Check RabbitMQ logs
kubectl logs <rabbitmq-pod-name> -n finmfb

# Check management interface
kubectl port-forward svc/rabbitmq 15672:15672 -n finmfb
```

Common causes:
- **RabbitMQ not ready**: RabbitMQ pod is not running
- **Authentication issues**: Incorrect credentials
- **Network policy**: Network policy blocking RabbitMQ traffic

## General Troubleshooting Commands

```bash
# Get all resources in namespace
kubectl get all -n finmfb

# Get resource utilization
kubectl top pods -n finmfb
kubectl top nodes

# Get logs from all containers in a pod
kubectl logs <pod-name> -n finmfb --all-containers

# Execute commands in a container
kubectl exec -it <pod-name> -n finmfb -- /bin/bash

# Port forward to a service
kubectl port-forward svc/<service-name> <local-port>:<service-port> -n finmfb

# Check events
kubectl get events -n finmfb --sort-by='.lastTimestamp'
```

## Helm Issues

```bash
# List Helm releases
helm list -n finmfb

# Check release status
helm status finmfb -n finmfb

# Check release history
helm history finmfb -n finmfb

# Rollback to previous release
helm rollback finmfb <revision> -n finmfb

# Debug template rendering
helm template finmfb ./helm/finmfb -f ./helm/finmfb/values-dev.yaml
```

## Common Fixes

### Restart a Deployment

```bash
kubectl rollout restart deployment <deployment-name> -n finmfb
```

### Scale a Deployment

```bash
kubectl scale deployment <deployment-name> --replicas=<count> -n finmfb
```

### Delete and Recreate a Pod

```bash
kubectl delete pod <pod-name> -n finmfb
```

### Update a ConfigMap and Restart Dependent Pods

```bash
kubectl edit configmap <configmap-name> -n finmfb
kubectl rollout restart deployment <deployment-name> -n finmfb
```

### Update a Secret and Restart Dependent Pods

```bash
kubectl create secret generic <secret-name> --from-literal=key=value -n finmfb --dry-run=client -o yaml | kubectl apply -f -
kubectl rollout restart deployment <deployment-name> -n finmfb
```