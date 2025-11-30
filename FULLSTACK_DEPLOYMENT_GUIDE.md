# 🚀 Full-Stack Deployment Guide

## Cooperative Loan Management System - Complete Deployment

**Status**: ✅ Backend + Frontend Fully Integrated  
**Architecture**: React Frontend + .NET Backend + SQL Server + Redis  

---

## 📋 System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Load Balancer / CDN                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        │                             │
┌───────▼────────┐           ┌────────▼────────┐
│   Frontend     │           │    Backend      │
│   (React)      │◄─────────►│    (.NET)       │
│   Port 3000    │   API     │    Port 5000    │
└────────────────┘           └────────┬────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
            ┌───────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐
            │  SQL Server  │  │    Redis    │  │   Hangfire  │
            │  Port 1433   │  │  Port 6379  │  │  (Jobs)     │
            └──────────────┘  └─────────────┘  └─────────────┘
```

---

## 🎯 Quick Start (All-in-One)

### Option 1: Docker Compose (Recommended)
```bash
# Clone repository
git clone <repository-url>
cd cooperative-loan-system

# Start all services
docker-compose -f docker-compose.fullstack.yml up -d

# Access applications
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Hangfire: http://localhost:5000/hangfire
```

### Option 2: Local Development
```bash
# Terminal 1: Start Backend
cd Fin-Backend
dotnet run

# Terminal 2: Start Frontend
cd frontend
npm install
npm run dev

# Access applications
# Frontend: http://localhost:3000
# Backend: http://localhost:5000
```

---

## 📦 Complete Docker Compose

Create `docker-compose.fullstack.yml`:

```yaml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: coop-loan-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - coop-loan-network
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"
      interval: 10s
      timeout: 3s
      retries: 10

  # Redis
  redis:
    image: redis:7-alpine
    container_name: coop-loan-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - coop-loan-network
    command: redis-server --appendonly yes
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  # Backend API
  backend:
    build:
      context: ./Fin-Backend
      dockerfile: Dockerfile
    container_name: coop-loan-backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CooperativeLoanDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
      - ConnectionStrings__RedisConnection=redis:6379
      - ConnectionStrings__HangfireConnection=Server=sqlserver;Database=CooperativeLoanHangfire;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    ports:
      - "5000:80"
    depends_on:
      sqlserver:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - coop-loan-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Frontend
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: coop-loan-frontend
    environment:
      - VITE_API_BASE_URL=http://localhost:5000/api
    ports:
      - "3000:80"
    depends_on:
      - backend
    networks:
      - coop-loan-network

volumes:
  sqlserver-data:
  redis-data:

networks:
  coop-loan-network:
    driver: bridge
```

---

## 🔧 Environment Configuration

### Backend (.env)
```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=CooperativeLoanDB;Trusted_Connection=True
ConnectionStrings__RedisConnection=localhost:6379
ConnectionStrings__HangfireConnection=Server=localhost;Database=CooperativeLoanHangfire;Trusted_Connection=True

# JWT
JwtSettings__SecretKey=YOUR_SECRET_KEY_MIN_32_CHARACTERS
JwtSettings__ExpiryMinutes=60

# Notifications
NotificationSettings__SendGridApiKey=YOUR_SENDGRID_KEY
NotificationSettings__TwilioAccountSid=YOUR_TWILIO_SID
NotificationSettings__TwilioAuthToken=YOUR_TWILIO_TOKEN

# Application Insights
ApplicationInsights__InstrumentationKey=YOUR_APP_INSIGHTS_KEY
```

### Frontend (.env)
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_NAME=Cooperative Loan Management
VITE_APP_VERSION=1.0.0
```

---

## 🚀 Deployment Steps

### Step 1: Prepare Environment
```bash
# Clone repository
git clone <repository-url>
cd cooperative-loan-system

# Create environment files
cp Fin-Backend/appsettings.example.json Fin-Backend/appsettings.json
cp frontend/.env.example frontend/.env

# Update configuration values
```

### Step 2: Deploy Backend
```bash
# Navigate to backend
cd Fin-Backend

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update

# Start backend
dotnet run

# Verify: http://localhost:5000/swagger
```

### Step 3: Deploy Frontend
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Verify: http://localhost:3000
```

### Step 4: Verify Integration
```bash
# Test API connection from frontend
curl http://localhost:3000

# Test backend health
curl http://localhost:5000/health

# Test API endpoint
curl http://localhost:5000/api/loan-calculator/calculate-emi \
  -H "Content-Type: application/json" \
  -d '{"principal":500000,"annualInterestRate":12,"tenureMonths":12}'
```

---

## 🐳 Docker Deployment

### Build Images
```bash
# Build backend image
docker build -t coop-loan-backend:1.0 ./Fin-Backend

# Build frontend image
docker build -t coop-loan-frontend:1.0 ./frontend
```

### Run with Docker Compose
```bash
# Start all services
docker-compose -f docker-compose.fullstack.yml up -d

# View logs
docker-compose -f docker-compose.fullstack.yml logs -f

# Stop services
docker-compose -f docker-compose.fullstack.yml down
```

---

## ☸️ Kubernetes Deployment

### Deploy Backend
```bash
kubectl apply -f k8s/backend/
```

### Deploy Frontend
```bash
kubectl apply -f k8s/frontend/
```

### Verify Deployment
```bash
kubectl get pods
kubectl get services
kubectl get ingress
```

---

## 🔐 Security Configuration

### HTTPS Setup
```bash
# Install certbot
sudo apt-get install certbot

# Generate SSL certificate
sudo certbot certonly --standalone -d yourdomain.com

# Configure nginx with SSL
```

### CORS Configuration
Backend `appsettings.json`:
```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://yourdomain.com"
    ]
  }
}
```

---

## 📊 Monitoring & Logging

### Application Insights
```bash
# Backend: Configured in appsettings.json
# Frontend: Add to index.html

<script>
  var appInsights = window.appInsights || function(config) {
    // Application Insights initialization
  }({
    instrumentationKey: "YOUR_KEY"
  });
</script>
```

### Health Checks
- **Backend**: http://localhost:5000/health
- **Frontend**: http://localhost:3000
- **Database**: SQL Server Management Studio
- **Redis**: redis-cli ping

---

## 🧪 Testing Full Stack

### End-to-End Test
```bash
# 1. Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# 2. Calculate EMI
curl -X POST http://localhost:5000/api/loan-calculator/calculate-emi \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"principal":500000,"annualInterestRate":12,"tenureMonths":12}'

# 3. Check Eligibility
curl -X POST http://localhost:5000/api/loan-eligibility/check \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"memberId":"MEM001","loanProductId":"PROD001","requestedAmount":1000000,"tenureMonths":12}'
```

---

## 📈 Performance Optimization

### Backend
- ✅ Redis caching enabled
- ✅ Database indexes optimized
- ✅ Connection pooling configured
- ✅ Async/await patterns used

### Frontend
- ✅ Code splitting with React.lazy
- ✅ Image optimization
- ✅ Bundle size optimization
- ✅ CDN for static assets

### Database
- ✅ Indexes on foreign keys
- ✅ Query optimization
- ✅ Connection pooling
- ✅ Regular maintenance

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow
```yaml
name: Full-Stack CI/CD

on:
  push:
    branches: [ main ]

jobs:
  build-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Backend
        run: |
          cd Fin-Backend
          dotnet build
          dotnet test
      - name: Build Docker Image
        run: docker build -t backend:latest ./Fin-Backend

  build-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Frontend
        run: |
          cd frontend
          npm install
          npm run build
      - name: Build Docker Image
        run: docker build -t frontend:latest ./frontend

  deploy:
    needs: [build-backend, build-frontend]
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Production
        run: |
          # Deploy commands here
```

---

## 📞 Troubleshooting

### Common Issues

#### 1. Frontend Can't Connect to Backend
```bash
# Check backend is running
curl http://localhost:5000/health

# Check CORS configuration
# Verify frontend URL in backend CORS settings

# Check proxy configuration in vite.config.ts
```

#### 2. Database Connection Failed
```bash
# Test SQL Server connection
sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Check connection string in appsettings.json
# Verify SQL Server is running
```

#### 3. Redis Connection Failed
```bash
# Test Redis
redis-cli ping

# Check Redis is running
docker ps | grep redis

# Verify connection string
```

---

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] Backend tests passing
- [ ] Frontend builds successfully
- [ ] Database migrations ready
- [ ] Environment variables configured
- [ ] SSL certificates obtained
- [ ] CORS configured correctly
- [ ] API keys configured

### Deployment
- [ ] Deploy database
- [ ] Deploy backend
- [ ] Deploy frontend
- [ ] Configure load balancer
- [ ] Set up monitoring
- [ ] Configure backups

### Post-Deployment
- [ ] Verify all services running
- [ ] Test critical workflows
- [ ] Check logs for errors
- [ ] Monitor performance
- [ ] Test from different devices
- [ ] Verify SSL working

---

## 🎉 Success!

Your full-stack Cooperative Loan Management System is now deployed and integrated!

### Access Points
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger Docs**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire

### Next Steps
1. ✅ Train users
2. ✅ Monitor system performance
3. ✅ Collect user feedback
4. ✅ Plan enhancements

---

**Version**: 1.0  
**Last Updated**: December 2024  
**Status**: ✅ Full-Stack Integrated and Deployed
