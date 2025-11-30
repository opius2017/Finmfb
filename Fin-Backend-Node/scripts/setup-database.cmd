@echo off
REM Batch script to setup SQL Server database for FinMFB using Windows Authentication

echo.
echo ========================================
echo   FinMFB Database Setup
echo   SQL Server 2022 - Windows Auth
echo ========================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] This script requires Administrator privileges.
    echo Please right-click and select "Run as Administrator"
    pause
    exit /b 1
)

echo [INFO] Running with Administrator privileges
echo.

REM Configuration
set SERVER=localhost
set DATABASE=FinMFBDb

echo [INFO] Server: %SERVER%
echo [INFO] Database: %DATABASE%
echo [INFO] Authentication: Windows Authentication
echo.

REM Test SQL Server connection
echo [STEP 1/5] Testing SQL Server connection...
sqlcmd -S %SERVER% -E -Q "SELECT @@VERSION" >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Cannot connect to SQL Server
    echo.
    echo Troubleshooting:
    echo 1. Ensure SQL Server 2022 is installed and running
    echo 2. Check that Windows Authentication is enabled
    echo 3. Verify your Windows user has SQL Server access
    echo.
    echo To check SQL Server status, run:
    echo    sc query MSSQLSERVER
    echo.
    pause
    exit /b 1
)
echo [SUCCESS] Connected to SQL Server
echo.

REM Check if database exists
echo [STEP 2/5] Checking if database exists...
sqlcmd -S %SERVER% -E -Q "SELECT database_id FROM sys.databases WHERE Name = '%DATABASE%'" -h -1 -W >nul 2>&1
if %errorLevel% equ 0 (
    echo [WARNING] Database '%DATABASE%' already exists
    set /p RECREATE="Do you want to drop and recreate it? (yes/no): "
    if /i "%RECREATE%"=="yes" (
        echo [INFO] Dropping existing database...
        sqlcmd -S %SERVER% -E -Q "USE master; ALTER DATABASE [%DATABASE%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [%DATABASE%];" >nul 2>&1
        if %errorLevel% equ 0 (
            echo [SUCCESS] Database dropped
        ) else (
            echo [ERROR] Failed to drop database
            pause
            exit /b 1
        )
    ) else (
        echo [INFO] Using existing database
        goto :SKIP_CREATE
    )
)

REM Create database
echo [STEP 3/5] Creating database...
sqlcmd -S %SERVER% -E -Q "CREATE DATABASE [%DATABASE%] COLLATE Latin1_General_100_CI_AS_SC_UTF8;" >nul 2>&1
if %errorLevel% equ 0 (
    echo [SUCCESS] Database created successfully
) else (
    echo [ERROR] Failed to create database
    pause
    exit /b 1
)

:SKIP_CREATE
echo.

REM Update .env file
echo [STEP 4/5] Updating .env file...
cd /d "%~dp0.."

if not exist ".env" (
    if exist ".env.example" (
        copy ".env.example" ".env" >nul
        echo [SUCCESS] Created .env from .env.example
    ) else (
        echo [ERROR] .env.example not found
        pause
        exit /b 1
    )
)

REM Update DATABASE_URL in .env
powershell -Command "(Get-Content .env) -replace 'DATABASE_URL=.*', 'DATABASE_URL=\"sqlserver://%SERVER%:1433;database=%DATABASE%;integratedSecurity=true;trustServerCertificate=true;encrypt=true\"' | Set-Content .env"
echo [SUCCESS] .env file updated
echo.

REM Install dependencies
echo [STEP 5/5] Setting up Node.js dependencies...
if not exist "node_modules" (
    echo [INFO] Installing npm packages...
    call npm install
    if %errorLevel% neq 0 (
        echo [ERROR] Failed to install dependencies
        echo Please run 'npm install' manually
        pause
        exit /b 1
    )
    echo [SUCCESS] Dependencies installed
) else (
    echo [INFO] Dependencies already installed
)
echo.

REM Run Prisma migrations
echo [INFO] Running Prisma migrations...
call npx prisma generate
if %errorLevel% neq 0 (
    echo [ERROR] Failed to generate Prisma client
    pause
    exit /b 1
)
echo [SUCCESS] Prisma client generated

call npx prisma db push --accept-data-loss
if %errorLevel% neq 0 (
    echo [ERROR] Failed to push database schema
    pause
    exit /b 1
)
echo [SUCCESS] Database schema pushed
echo.

REM Seed database
echo [INFO] Seeding database with Nigerian data...
call npx ts-node prisma/seed.ts
if %errorLevel% equ 0 (
    echo [SUCCESS] Database seeded successfully
) else (
    echo [WARNING] Failed to seed database
    echo You can manually run: npx ts-node prisma/seed.ts
)
echo.

REM Success message
echo ========================================
echo   Setup Completed Successfully!
echo ========================================
echo.
echo Database Details:
echo   Server:       %SERVER%
echo   Database:     %DATABASE%
echo   Auth Method:  Windows Authentication
echo   Status:       Ready
echo.
echo Default Login Credentials:
echo   Admin:         admin@finmfb.ng / Password123!
echo   Manager:       manager@finmfb.ng / Password123!
echo   Teller:        teller@finmfb.ng / Password123!
echo   Loan Officer:  loanofficer@finmfb.ng / Password123!
echo   Accountant:    accountant@finmfb.ng / Password123!
echo.
echo Next Steps:
echo   1. Start the server:    npm run dev
echo   2. View API docs:       http://localhost:3000/api-docs
echo   3. Browse database:     npm run db:studio
echo.
echo Happy coding!
echo.
pause
