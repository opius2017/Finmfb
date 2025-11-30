# ============================================
# Setup and Seed SQL Server Database
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Soar MFB Database Setup and Seeding" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set connection parameters
$Server = "localhost"
$Database = "SoarMFBDb"
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Function to execute SQL script
function Execute-SqlScript {
    param(
        [string]$ScriptFile,
        [string]$DatabaseName = "master"
    )
    
    $FullPath = Join-Path $ScriptPath $ScriptFile
    
    if (-not (Test-Path $FullPath)) {
        Write-Host "ERROR: Script file not found: $FullPath" -ForegroundColor Red
        return $false
    }
    
    try {
        if ($DatabaseName -eq "master") {
            sqlcmd -S $Server -E -i $FullPath
        } else {
            sqlcmd -S $Server -d $DatabaseName -E -i $FullPath
        }
        
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        return $true
    }
    catch {
        Write-Host "ERROR: $_" -ForegroundColor Red
        return $false
    }
}

# Step 1: Create Database
Write-Host "Step 1: Creating database..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "create-database.sql" -DatabaseName "master") {
    Write-Host "Database created successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to create database" -ForegroundColor Red
    exit 1
}

# Step 2: Create Schema
Write-Host "Step 2: Creating schema..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "create-schema.sql" -DatabaseName $Database) {
    Write-Host "Schema created successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to create schema" -ForegroundColor Red
    exit 1
}

# Step 3: Seed Users and Roles
Write-Host "Step 3: Seeding users and roles..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "seed-01-users-roles.sql" -DatabaseName $Database) {
    Write-Host "Users and roles seeded successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to seed users and roles" -ForegroundColor Red
    exit 1
}

# Step 4: Seed Branches and Members
Write-Host "Step 4: Seeding branches and members..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "seed-02-branches-members.sql" -DatabaseName $Database) {
    Write-Host "Branches and members seeded successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to seed branches and members" -ForegroundColor Red
    exit 1
}

# Step 5: Seed Accounts and Transactions
Write-Host "Step 5: Seeding accounts and transactions..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "seed-03-accounts-transactions.sql" -DatabaseName $Database) {
    Write-Host "Accounts and transactions seeded successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to seed accounts and transactions" -ForegroundColor Red
    exit 1
}

# Step 6: Seed Loan Products and Loans
Write-Host "Step 6: Seeding loan products and loans..." -ForegroundColor Yellow
if (Execute-SqlScript -ScriptFile "seed-04-loans.sql" -DatabaseName $Database) {
    Write-Host "Loans seeded successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "ERROR: Failed to seed loans" -ForegroundColor Red
    exit 1
}

# Success message
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database setup and seeding completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database: $Database" -ForegroundColor White
Write-Host "Server: $Server" -ForegroundColor White
Write-Host ""
Write-Host "You can now start the application with: npm run dev" -ForegroundColor Yellow
Write-Host ""
