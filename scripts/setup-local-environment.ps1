# Cooperative Loan Management System - Local Environment Setup Script
# Run this script to set up your local development environment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cooperative Loan Management System" -ForegroundColor Cyan
Write-Host "Local Environment Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Check SQL Server
try {
    $sqlCheck = sqlcmd -S localhost -Q "SELECT @@VERSION" -b
    Write-Host "✓ SQL Server is accessible" -ForegroundColor Green
} catch {
    Write-Host "✗ SQL Server not accessible. Please install SQL Server" -ForegroundColor Red
    Write-Host "  You can use SQL Server Express or Docker" -ForegroundColor Yellow
}

# Check Docker (optional)
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "⚠ Docker not found (optional)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Setting up project..." -ForegroundColor Yellow

# Navigate to project directory
$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore Fin-Backend/Fin-Backend.csproj
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Package restore failed" -ForegroundColor Red
    exit 1
}

# Create database
Write-Host ""
Write-Host "Setting up database..." -ForegroundColor Yellow
sqlcmd -S localhost -i database/init-database.sql
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database created successfully" -ForegroundColor Green
} else {
    Write-Host "⚠ Database creation had issues (may already exist)" -ForegroundColor Yellow
}

# Run EF Core migrations
Write-Host ""
Write-Host "Running database migrations..." -ForegroundColor Yellow
Set-Location Fin-Backend
dotnet ef database update
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migrations applied successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Migration failed" -ForegroundColor Red
    Set-Location $projectRoot
    exit 1
}
Set-Location $projectRoot

# Create logs directory
Write-Host ""
Write-Host "Creating logs directory..." -ForegroundColor Yellow
$logsDir = Join-Path $projectRoot "Fin-Backend\Logs"
if (!(Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir | Out-Null
    Write-Host "✓ Logs directory created" -ForegroundColor Green
} else {
    Write-Host "✓ Logs directory already exists" -ForegroundColor Green
}

# Setup complete
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.json with your configuration"
Write-Host "2. Run the application: dotnet run --project Fin-Backend"
Write-Host "3. Access Swagger UI: https://localhost:5001/swagger"
Write-Host "4. Access Hangfire Dashboard: https://localhost:5001/hangfire"
Write-Host ""
Write-Host "For Docker setup, run: docker-compose up -d" -ForegroundColor Cyan
Write-Host ""
