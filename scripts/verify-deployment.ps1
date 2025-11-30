# Cooperative Loan Management System - Deployment Verification Script
# Run this script to verify that the deployment is successful

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"
$passed = 0
$failed = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus = 200
    )
    
    Write-Host "Testing: $Name..." -NoNewline
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Host " ✓ PASSED" -ForegroundColor Green
            return $true
        } else {
            Write-Host " ✗ FAILED (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host " ✗ FAILED ($($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

# Health Check
Write-Host "1. Health Checks" -ForegroundColor Yellow
Write-Host "----------------------------------------"
if (Test-Endpoint "Application Health" "$baseUrl/health") { $passed++ } else { $failed++ }
Write-Host ""

# API Endpoints
Write-Host "2. API Endpoints" -ForegroundColor Yellow
Write-Host "----------------------------------------"
if (Test-Endpoint "Swagger UI" "$baseUrl/swagger/index.html") { $passed++ } else { $failed++ }
if (Test-Endpoint "Hangfire Dashboard" "$baseUrl/hangfire") { $passed++ } else { $failed++ }
Write-Host ""

# Database Connectivity
Write-Host "3. Database Connectivity" -ForegroundColor Yellow
Write-Host "----------------------------------------"
Write-Host "Testing SQL Server connection..." -NoNewline
try {
    $sqlTest = sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" -b 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " ✓ PASSED" -ForegroundColor Green
        $passed++
    } else {
        Write-Host " ✗ FAILED" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host " ✗ FAILED" -ForegroundColor Red
    $failed++
}
Write-Host ""

# Redis Connectivity
Write-Host "4. Redis Connectivity" -ForegroundColor Yellow
Write-Host "----------------------------------------"
Write-Host "Testing Redis connection..." -NoNewline
try {
    $redisTest = docker exec cooperative-loan-redis redis-cli ping 2>&1
    if ($redisTest -eq "PONG") {
        Write-Host " ✓ PASSED" -ForegroundColor Green
        $passed++
    } else {
        Write-Host " ✗ FAILED" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host " ✗ FAILED" -ForegroundColor Red
    $failed++
}
Write-Host ""

# Background Jobs
Write-Host "5. Background Jobs" -ForegroundColor Yellow
Write-Host "----------------------------------------"
Write-Host "Checking Hangfire jobs..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/admin/background-jobs/recurring" -UseBasicParsing
    $jobs = $response.Content | ConvertFrom-Json
    if ($jobs.Count -gt 0) {
        Write-Host " ✓ PASSED ($($jobs.Count) jobs registered)" -ForegroundColor Green
        $passed++
    } else {
        Write-Host " ⚠ WARNING (No jobs registered)" -ForegroundColor Yellow
        $failed++
    }
} catch {
    Write-Host " ✗ FAILED" -ForegroundColor Red
    $failed++
}
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failed -eq 0) {
    Write-Host "✓ All checks passed! Deployment is successful." -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Yellow
    Write-Host "  1. Access Swagger UI: $baseUrl/swagger" -ForegroundColor Cyan
    Write-Host "  2. Access Hangfire Dashboard: $baseUrl/hangfire" -ForegroundColor Cyan
    Write-Host "  3. Start using the API" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "✗ Some checks failed. Please review the errors above." -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check container logs: docker-compose logs" -ForegroundColor Cyan
    Write-Host "  2. Verify configuration in appsettings.json" -ForegroundColor Cyan
    Write-Host "  3. Ensure all services are running: docker-compose ps" -ForegroundColor Cyan
    exit 1
}
