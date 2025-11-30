# Cooperative Loan Management System - Docker Deployment Script
# Run this script to deploy the application using Docker Compose

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Docker Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop" -ForegroundColor Red
    exit 1
}

# Stop existing containers
Write-Host ""
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

# Build and start containers
Write-Host ""
Write-Host "Building and starting containers..." -ForegroundColor Yellow
docker-compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Containers started successfully" -ForegroundColor Green
    
    # Wait for services to be ready
    Write-Host ""
    Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Check container status
    Write-Host ""
    Write-Host "Container Status:" -ForegroundColor Cyan
    docker-compose ps
    
    # Run database migrations
    Write-Host ""
    Write-Host "Running database migrations..." -ForegroundColor Yellow
    docker-compose exec api dotnet ef database update
    
    # Health check
    Write-Host ""
    Write-Host "Performing health check..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ Application is healthy" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠ Health check failed. Application may still be starting..." -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Application URLs:" -ForegroundColor Yellow
    Write-Host "  API: http://localhost:5000" -ForegroundColor Cyan
    Write-Host "  Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
    Write-Host "  Hangfire: http://localhost:5000/hangfire" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Database:" -ForegroundColor Yellow
    Write-Host "  SQL Server: localhost:1433" -ForegroundColor Cyan
    Write-Host "  Username: sa" -ForegroundColor Cyan
    Write-Host "  Password: YourStrong@Passw0rd" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Redis:" -ForegroundColor Yellow
    Write-Host "  Host: localhost:6379" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To view logs: docker-compose logs -f" -ForegroundColor Yellow
    Write-Host "To stop: docker-compose down" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "✗ Deployment failed" -ForegroundColor Red
    Write-Host "Check logs with: docker-compose logs" -ForegroundColor Yellow
    exit 1
}
