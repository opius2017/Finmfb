# 🚀 Complete Deployment Guide

## Cooperative Loan Management System - Production Deployment

This guide provides step-by-step instructions for deploying the Cooperative Loan Management System to production.

---

## 📋 Prerequisites

### Required Software
- ✅ .NET 8.0 SDK or later
- ✅ SQL Server 2019 or later
- ✅ Redis 7.0 or later
- ✅ Docker Desktop (for containerized deployment)
- ✅ Kubernetes cluster (for K8s deployment)

### Required Accounts
- ✅ SendGrid account (for email notifications)
- ✅ Twilio account (for SMS notifications)
- ✅ Azure subscription (for Application Insights)
- ✅ Container registry (Azure ACR, Docker Hub, etc.)

---

## 🎯 Deployment Options

### Option 1: Local Development Setup
**Best for**: Development and testing
**Time**: 15 minutes

### Option 2: Docker Compose
**Best for**: Quick deployment, staging environments
**Time**: 20 minutes

### Option 3: Kubernetes
**Best for**: Production, high availability
**Time**: 45 minutes

---

## 📦 Option 1: Local Development Setup

### Step 1: Clone Repository
```powershell
git clone <repository-url>
cd cooperative-loan-system
```

### Step 2: Run Setup Script
```powershell
.\scripts\setup-local-environment.ps1
```

This script will:
- ✅ Check prerequisites
- ✅ Restore NuGet packages
- ✅ Create databases
- ✅ Run migrations
- ✅ Create logs directory

### Step 3: Configure Application
Edit `Fin-Backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CooperativeLoanDB;Trusted_Connection=True;",
    "RedisConnection": "localhost:6379",
    "HangfireConnection": "Server=localhost;Database=CooperativeLoanHangfire;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS"
  },
  "NotificationSettings": {
    "SendGridApiKey": "YOUR_SENDGRID_KEY",
    "TwilioAccountSid": "YOUR_TWILIO_SID",
    "TwilioAuthToken": "YOUR_TWILIO_TOKEN"
  }
}
```

### Step 4: Run Application
```powershell
cd Fin-Backend
dotnet run
```

### Step 5: Verify Deployment
```powershell
.\scripts\verify-deployment.ps1
```

### Step 6: Access Application
- **Swagger UI**: https://localhost:5001/swagger
- **Hangfire Dashboard**: https://localhost:5001/hangfire
- **API Base**: https://localhost:5001/api

---

## 🐳 Option 2: Docker Compose Deployment

### Step 1: Install Docker Desktop
Download and install from: https://www.docker.com/products/docker-desktop

### Step 2: Configure Environment
Edit `docker-compose.yml` if needed (default settings work for local deployment)

### Step 3: Deploy with Script
```powershell
.\scripts\deploy-to-docker.ps1
```

This script will:
- ✅ Stop existing containers
- ✅ Build Docker images
- ✅ Start all services (SQL Server, Redis, API)
- ✅ Run database migrations
- ✅ Perform health checks

### Step 4: Verify Deployment
```powershell
.\scripts\verify-deployment.ps1
```

### Step 5: Access Application
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Hangfire**: http://localhost:5000/hangfire
- **SQL Server**: localhost:1433 (sa / YourStrong@Passw0rd)
- **Redis**: localhost:6379

### Step 6: View Logs
```powershell
# View all logs
docker-compose logs -f

# View API logs only
docker-compose logs -f api

# View SQL Server logs
docker-compose logs -f sqlserver
```

### Step 7: Stop Services
```powershell
docker-compose down
```

---

## ☸️ Option 3: Kubernetes Deployment

### Step 1: Prepare Kubernetes Cluster
```bash
# For Azure AKS
az aks get-credentials --resource-group cooperative-loan-rg --name cooperative-loan-aks

# For local Kubernetes (Docker Desktop)
kubectl config use-context docker-desktop
```

### Step 2: Create Namespace
```bash
kubectl create namespace cooperative-loan
kubectl config set-context --current --namespace=cooperative-loan
```

### Step 3: Configure Secrets
Edit `k8s/secrets.yaml` with your actual credentials:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: cooperative-loan-secrets
type: Opaque
stringData:
  database-connection: "Server=YOUR_SQL;Database=CooperativeLoanDB;User Id=YOUR_USER;Password=YOUR_PASS"
  redis-connection: "YOUR_REDIS:6379,password=YOUR_PASS"
  jwt-secret: "YOUR_JWT_SECRET_32_CHARS_MIN"
  sendgrid-api-key: "YOUR_SENDGRID_KEY"
  twilio-account-sid: "YOUR_TWILIO_SID"
  twilio-auth-token: "YOUR_TWILIO_TOKEN"
```

### Step 4: Apply Kubernetes Manifests
```bash
# Apply secrets
kubectl apply -f k8s/secrets.yaml

# Apply config map
kubectl apply -f k8s/configmap.yaml

# Deploy application
kubectl apply -f k8s/deployment.yaml
```

### Step 5: Verify Deployment
```bash
# Check pods
kubectl get pods

# Check services
kubectl get services

# Check deployment status
kubectl rollout status deployment/cooperative-loan-api

# View logs
kubectl logs -f deployment/cooperative-loan-api
```

### Step 6: Access Application
```bash
# Get external IP
kubectl get service cooperative-loan-api-service

# Access application
# http://<EXTERNAL-IP>
```

### Step 7: Scale Application
```bash
# Manual scaling
kubectl scale deployment cooperative-loan-api --replicas=5

# Auto-scaling is configured via HPA (3-10 replicas)
kubectl get hpa
```

---

## 🔧 Configuration Guide

### Database Configuration

#### SQL Server
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CooperativeLoanDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"
}
```

#### Redis
```json
"ConnectionStrings": {
  "RedisConnection": "YOUR_REDIS_HOST:6379,password=YOUR_PASSWORD,ssl=True,abortConnect=False"
}
```

### JWT Configuration
```json
"JwtSettings": {
  "SecretKey": "MINIMUM_32_CHARACTERS_SECRET_KEY_CHANGE_THIS",
  "Issuer": "CooperativeLoanAPI",
  "Audience": "CooperativeLoanClients",
  "ExpiryMinutes": 30,
  "RefreshTokenExpiryDays": 7
}
```

### Notification Configuration
```json
"NotificationSettings": {
  "EmailProvider": "SendGrid",
  "SmsProvider": "Twilio",
  "SendGridApiKey": "YOUR_SENDGRID_API_KEY",
  "SendGridFromEmail": "noreply@yourdomain.com",
  "TwilioAccountSid": "YOUR_TWILIO_ACCOUNT_SID",
  "TwilioAuthToken": "YOUR_TWILIO_AUTH_TOKEN",
  "TwilioPhoneNumber": "+1234567890"
}
```

### CORS Configuration
```json
"CorsSettings": {
  "AllowedOrigins": [
    "https://yourdomain.com",
    "https://app.yourdomain.com"
  ]
}
```

---

## 🧪 Testing Deployment

### Run All Tests
```powershell
.\scripts\run-tests.ps1
```

### Manual API Testing

#### 1. Health Check
```bash
curl http://localhost:5000/health
```

#### 2. Calculate EMI
```bash
curl -X POST "http://localhost:5000/api/loan-calculator/calculate-emi" \
  -H "Content-Type: application/json" \
  -d '{
    "principal": 500000,
    "annualInterestRate": 12,
    "tenureMonths": 12
  }'
```

#### 3. Check Background Jobs
```bash
curl http://localhost:5000/api/admin/background-jobs/recurring
```

---

## 📊 Monitoring & Logging

### Application Insights
Configure in `appsettings.json`:
```json
"ApplicationInsights": {
  "InstrumentationKey": "YOUR_KEY"
}
```

### View Logs
```powershell
# Local deployment
Get-Content Fin-Backend\Logs\log-*.txt -Tail 100 -Wait

# Docker deployment
docker-compose logs -f api

# Kubernetes deployment
kubectl logs -f deployment/cooperative-loan-api
```

### Hangfire Dashboard
Access at: `http://your-domain/hangfire`

Monitor:
- ✅ Recurring jobs
- ✅ Job execution history
- ✅ Failed jobs
- ✅ Server statistics

---

## 🔒 Security Checklist

### Before Production Deployment
- [ ] Change all default passwords
- [ ] Generate new JWT secret key (min 32 characters)
- [ ] Configure HTTPS/SSL certificates
- [ ] Enable two-factor authentication
- [ ] Configure firewall rules
- [ ] Set up rate limiting
- [ ] Enable audit logging
- [ ] Configure backup jobs
- [ ] Review CORS settings
- [ ] Update allowed origins

---

## 🔄 Database Migrations

### Apply Migrations
```powershell
# Local
cd Fin-Backend
dotnet ef database update

# Docker
docker-compose exec api dotnet ef database update

# Kubernetes
kubectl exec -it deployment/cooperative-loan-api -- dotnet ef database update
```

### Create New Migration
```powershell
dotnet ef migrations add MigrationName --project Fin-Backend
```

### Rollback Migration
```powershell
dotnet ef database update PreviousMigrationName --project Fin-Backend
```

---

## 📦 Backup & Recovery

### Database Backup
```sql
-- Full backup
BACKUP DATABASE CooperativeLoanDB
TO DISK = 'C:\Backups\CooperativeLoanDB_Full.bak'
WITH FORMAT, INIT, NAME = 'Full Backup';

-- Differential backup
BACKUP DATABASE CooperativeLoanDB
TO DISK = 'C:\Backups\CooperativeLoanDB_Diff.bak'
WITH DIFFERENTIAL, INIT, NAME = 'Differential Backup';
```

### Database Restore
```sql
RESTORE DATABASE CooperativeLoanDB
FROM DISK = 'C:\Backups\CooperativeLoanDB_Full.bak'
WITH REPLACE, RECOVERY;
```

---

## 🚨 Troubleshooting

### Common Issues

#### 1. Database Connection Failed
```powershell
# Test connection
sqlcmd -S YOUR_SERVER -U YOUR_USER -P YOUR_PASSWORD -Q "SELECT 1"

# Check connection string in appsettings.json
# Verify firewall rules
# Ensure SQL Server is running
```

#### 2. Redis Connection Failed
```powershell
# Test Redis
redis-cli ping

# Check Redis is running
docker ps | grep redis

# Verify connection string
```

#### 3. Background Jobs Not Running
```powershell
# Check Hangfire dashboard
# Verify Hangfire connection string
# Check job registration in logs
# Ensure EnableBackgroundJobs is true
```

#### 4. API Returns 500 Error
```powershell
# Check logs
Get-Content Fin-Backend\Logs\log-*.txt -Tail 50

# Check Application Insights
# Verify all configuration settings
# Check database connectivity
```

---

## 📞 Support

### Documentation
- **API Docs**: https://your-domain/swagger
- **Deployment Checklist**: DEPLOYMENT_CHECKLIST.md
- **Quick Reference**: QUICK_REFERENCE_GUIDE.md

### Contact
- **Technical Support**: support@yourdomain.com
- **Emergency**: +1-XXX-XXX-XXXX

---

## ✅ Post-Deployment Checklist

### Immediate (Day 1)
- [ ] Verify all services running
- [ ] Check application logs
- [ ] Test critical workflows
- [ ] Verify background jobs
- [ ] Monitor error rates
- [ ] Test notifications

### Week 1
- [ ] Review user feedback
- [ ] Monitor performance metrics
- [ ] Check error logs daily
- [ ] Verify data accuracy
- [ ] Review security logs

### Month 1
- [ ] Analyze usage patterns
- [ ] Optimize slow queries
- [ ] Address user feedback
- [ ] Update documentation
- [ ] Plan enhancements

---

## 🎉 Success!

Your Cooperative Loan Management System is now deployed and ready for use!

**Next Steps**:
1. ✅ Train users
2. ✅ Monitor system performance
3. ✅ Collect user feedback
4. ✅ Plan enhancements

---

**Version**: 1.0  
**Last Updated**: December 2024  
**Status**: Production Ready
