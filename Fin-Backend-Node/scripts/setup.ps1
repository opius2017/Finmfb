# Setup script for FinTech Backend API (PowerShell)

Write-Host "🚀 Setting up FinTech Backend API..." -ForegroundColor Green

# Check Node.js version
Write-Host "📦 Checking Node.js version..." -ForegroundColor Cyan
$nodeVersion = node -v
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Node.js is not installed" -ForegroundColor Red
    exit 1
}
$nodeMajorVersion = [int]($nodeVersion -replace 'v(\d+)\..*', '$1')
if ($nodeMajorVersion -lt 20) {
    Write-Host "❌ Node.js version 20 or higher is required" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green

# Check npm version
Write-Host "📦 Checking npm version..." -ForegroundColor Cyan
$npmVersion = npm -v
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ npm is not installed" -ForegroundColor Red
    exit 1
}
$npmMajorVersion = [int]($npmVersion -split '\.')[0]
if ($npmMajorVersion -lt 10) {
    Write-Host "❌ npm version 10 or higher is required" -ForegroundColor Red
    exit 1
}
Write-Host "✅ npm version: $npmVersion" -ForegroundColor Green

# Install dependencies
Write-Host "📦 Installing dependencies..." -ForegroundColor Cyan
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install dependencies" -ForegroundColor Red
    exit 1
}

# Copy environment file if it doesn't exist
if (-not (Test-Path .env)) {
    Write-Host "📝 Creating .env file from .env.example..." -ForegroundColor Cyan
    Copy-Item .env.example .env
    Write-Host "⚠️  Please update .env file with your configuration" -ForegroundColor Yellow
}

# Setup Husky
Write-Host "🐶 Setting up Husky..." -ForegroundColor Cyan
npm run prepare

# Generate Prisma client
Write-Host "🔧 Generating Prisma client..." -ForegroundColor Cyan
npx prisma generate

# Create logs directory
Write-Host "📁 Creating logs directory..." -ForegroundColor Cyan
if (-not (Test-Path logs)) {
    New-Item -ItemType Directory -Path logs | Out-Null
}

Write-Host ""
Write-Host "✅ Setup completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update .env file with your configuration"
Write-Host "2. Start PostgreSQL and Redis (or use docker-compose up -d)"
Write-Host "3. Run database migrations: npm run migrate"
Write-Host "4. Seed database: npm run db:seed"
Write-Host "5. Start development server: npm run dev"
Write-Host ""
Write-Host "For Docker setup:" -ForegroundColor Cyan
Write-Host "  docker-compose up -d"
Write-Host ""
