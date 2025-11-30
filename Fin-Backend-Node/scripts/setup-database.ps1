# PowerShell script to setup SQL Server database for FinMFB using Windows Authentication

Write-Host "🚀 Setting up FinMFB Database on SQL Server 2022..." -ForegroundColor Green
Write-Host ""

# Configuration
$ServerInstance = "localhost"
$DatabaseName = "FinMFBDb"

Write-Host "ℹ️  Using Windows Authentication" -ForegroundColor Cyan
Write-Host "   Current User: $env:USERDOMAIN\$env:USERNAME" -ForegroundColor Cyan
Write-Host ""

# Test SQL Server connection
Write-Host "📡 Testing SQL Server connection..." -ForegroundColor Yellow
try {
    $connectionString = "Server=$ServerInstance;Integrated Security=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "✅ SQL Server connection successful!" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "❌ Failed to connect to SQL Server: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting Tips:" -ForegroundColor Yellow
    Write-Host "1. Ensure SQL Server 2022 is running" -ForegroundColor White
    Write-Host "2. Verify Windows Authentication is enabled" -ForegroundColor White
    Write-Host "3. Check that your Windows user has SQL Server access" -ForegroundColor White
    Write-Host "4. Run PowerShell as Administrator if needed" -ForegroundColor White
    Write-Host ""
    Write-Host "To check SQL Server status, run:" -ForegroundColor Yellow
    Write-Host "   Get-Service -Name 'MSSQL*'" -ForegroundColor White
    exit 1
}

# Check if database exists
Write-Host "🔍 Checking if database exists..." -ForegroundColor Yellow
$checkDbQuery = "SELECT database_id FROM sys.databases WHERE Name = '$DatabaseName'"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = $checkDbQuery
$result = $command.ExecuteScalar()
$connection.Close()

if ($result) {
    Write-Host "⚠️  Database '$DatabaseName' already exists." -ForegroundColor Yellow
    $response = Read-Host "Do you want to drop and recreate it? (yes/no)"
    if ($response -eq "yes") {
        Write-Host "🗑️  Dropping existing database..." -ForegroundColor Yellow
        $dropDbQuery = @"
USE master;
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$DatabaseName];
"@
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $dropDbQuery
        try {
            $command.ExecuteNonQuery()
            Write-Host "✅ Database dropped successfully!" -ForegroundColor Green
        } catch {
            Write-Host "❌ Failed to drop database: $_" -ForegroundColor Red
            $connection.Close()
            exit 1
        }
        $connection.Close()
    } else {
        Write-Host "ℹ️  Using existing database." -ForegroundColor Cyan
    }
}

# Create database if it doesn't exist
if (-not $result -or $response -eq "yes") {
    Write-Host "📦 Creating database '$DatabaseName'..." -ForegroundColor Yellow
    $createDbQuery = @"
CREATE DATABASE [$DatabaseName]
COLLATE Latin1_General_100_CI_AS_SC_UTF8;
"@
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = $createDbQuery
    try {
        $command.ExecuteNonQuery()
        Write-Host "✅ Database created successfully!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to create database: $_" -ForegroundColor Red
        $connection.Close()
        exit 1
    }
    $connection.Close()
}

# Update .env file
Write-Host "📝 Updating .env file..." -ForegroundColor Yellow
$envPath = Join-Path $PSScriptRoot "..\..\.env"
$envExamplePath = Join-Path $PSScriptRoot "..\.env.example"

# Create .env from .env.example if it doesn't exist
if (-not (Test-Path $envPath)) {
    if (Test-Path $envExamplePath) {
        Copy-Item $envExamplePath $envPath
        Write-Host "✅ Created .env file from .env.example" -ForegroundColor Green
    } else {
        Write-Host "❌ .env.example not found!" -ForegroundColor Red
        exit 1
    }
}

# Update DATABASE_URL in .env with Windows Authentication
$databaseUrl = "sqlserver://$ServerInstance`:1433;database=$DatabaseName;integratedSecurity=true;trustServerCertificate=true;encrypt=true"
$envContent = Get-Content $envPath -Raw
$envContent = $envContent -replace 'DATABASE_URL=.*', "DATABASE_URL=`"$databaseUrl`""
$envContent | Set-Content $envPath -NoNewline

Write-Host "✅ .env file updated with Windows Authentication connection string" -ForegroundColor Green

# Navigate to project directory
Write-Host ""
Write-Host "📦 Installing dependencies..." -ForegroundColor Yellow
Set-Location (Join-Path $PSScriptRoot "..")

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    try {
        npm install
        Write-Host "✅ Dependencies installed!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to install dependencies: $_" -ForegroundColor Red
        Write-Host "Please run 'npm install' manually" -ForegroundColor Yellow
    }
} else {
    Write-Host "✅ Dependencies already installed" -ForegroundColor Green
}

# Run Prisma migrations
Write-Host ""
Write-Host "🔄 Running Prisma migrations..." -ForegroundColor Yellow

try {
    Write-Host "Generating Prisma client..." -ForegroundColor Yellow
    npx prisma generate
    Write-Host "✅ Prisma client generated!" -ForegroundColor Green
    
    Write-Host "Pushing database schema..." -ForegroundColor Yellow
    npx prisma db push --accept-data-loss
    Write-Host "✅ Database schema pushed!" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to run Prisma migrations: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "You can manually run:" -ForegroundColor Yellow
    Write-Host "   npx prisma generate" -ForegroundColor White
    Write-Host "   npx prisma db push" -ForegroundColor White
    exit 1
}

# Seed database
Write-Host ""
Write-Host "🌱 Seeding database with Nigerian data..." -ForegroundColor Yellow
try {
    npx ts-node prisma/seed.ts
    Write-Host "✅ Database seeded successfully!" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Failed to seed database: $_" -ForegroundColor Yellow
    Write-Host "You can manually run: npx ts-node prisma/seed.ts" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "🎉 Database setup completed successfully!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Database Details:" -ForegroundColor Cyan
Write-Host "   Server:       $ServerInstance" -ForegroundColor White
Write-Host "   Database:     $DatabaseName" -ForegroundColor White
Write-Host "   Auth Method:  Windows Authentication" -ForegroundColor White
Write-Host "   Status:       Ready ✓" -ForegroundColor White
Write-Host ""
Write-Host "🔑 Default Login Credentials:" -ForegroundColor Cyan
Write-Host "   Admin:         admin@finmfb.ng / Password123!" -ForegroundColor White
Write-Host "   Manager:       manager@finmfb.ng / Password123!" -ForegroundColor White
Write-Host "   Teller:        teller@finmfb.ng / Password123!" -ForegroundColor White
Write-Host "   Loan Officer:  loanofficer@finmfb.ng / Password123!" -ForegroundColor White
Write-Host "   Accountant:    accountant@finmfb.ng / Password123!" -ForegroundColor White
Write-Host ""
Write-Host "📊 Sample Data Included:" -ForegroundColor Cyan
Write-Host "   ✓ 5 Branches (Lagos, Abuja, Port Harcourt, Kano, Ibadan)" -ForegroundColor White
Write-Host "   ✓ 20 Members with Nigerian names" -ForegroundColor White
Write-Host "   ✓ 30+ Accounts (Savings & Shares)" -ForegroundColor White
Write-Host "   ✓ 50+ Transactions" -ForegroundColor White
Write-Host "   ✓ 5 Loan Products" -ForegroundColor White
Write-Host "   ✓ 15 Loans" -ForegroundColor White
Write-Host "   ✓ 2024 Budget" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Start the server:    npm run dev" -ForegroundColor White
Write-Host "   2. View API docs:       http://localhost:3000/api-docs" -ForegroundColor White
Write-Host "   3. Browse database:     npm run db:studio" -ForegroundColor White
Write-Host ""
Write-Host "Happy coding! 🎉" -ForegroundColor Green
