# Cooperative Loan Management System - Test Runner Script
# Run this script to execute all tests with coverage

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running Tests with Coverage" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

# Clean previous test results
Write-Host "Cleaning previous test results..." -ForegroundColor Yellow
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
}

# Run tests with coverage
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test Fin-Backend.Tests/Fin-Backend.Tests.csproj `
    --configuration Release `
    --logger "console;verbosity=detailed" `
    --collect:"XPlat Code Coverage" `
    --results-directory ./TestResults

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ All tests passed!" -ForegroundColor Green
    
    # Generate coverage report
    Write-Host ""
    Write-Host "Generating coverage report..." -ForegroundColor Yellow
    
    # Install ReportGenerator if not already installed
    dotnet tool install --global dotnet-reportgenerator-globaltool 2>$null
    
    # Generate HTML report
    reportgenerator `
        "-reports:TestResults/**/coverage.cobertura.xml" `
        "-targetdir:TestResults/CoverageReport" `
        "-reporttypes:Html;HtmlSummary"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Coverage report generated" -ForegroundColor Green
        Write-Host ""
        Write-Host "Coverage report location: TestResults/CoverageReport/index.html" -ForegroundColor Cyan
        
        # Open report in browser
        $reportPath = Join-Path $projectRoot "TestResults\CoverageReport\index.html"
        if (Test-Path $reportPath) {
            Write-Host "Opening coverage report in browser..." -ForegroundColor Yellow
            Start-Process $reportPath
        }
    }
} else {
    Write-Host ""
    Write-Host "✗ Some tests failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test execution completed" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
