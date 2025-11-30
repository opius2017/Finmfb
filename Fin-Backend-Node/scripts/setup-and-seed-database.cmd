@echo off
REM ============================================
REM Setup and Seed SQL Server Database
REM ============================================

echo ========================================
echo Soar MFB Database Setup and Seeding
echo ========================================
echo.

REM Set connection string
set SERVER=localhost
set DATABASE=SoarMFBDb

echo Step 1: Creating database...
sqlcmd -S %SERVER% -E -i "%~dp0create-database.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to create database
    pause
    exit /b 1
)
echo Database created successfully!
echo.

echo Step 2: Creating schema...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%~dp0create-schema.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to create schema
    pause
    exit /b 1
)
echo Schema created successfully!
echo.

echo Step 3: Seeding users and roles...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%~dp0seed-01-users-roles.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to seed users and roles
    pause
    exit /b 1
)
echo Users and roles seeded successfully!
echo.

echo Step 4: Seeding branches and members...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%~dp0seed-02-branches-members.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to seed branches and members
    pause
    exit /b 1
)
echo Branches and members seeded successfully!
echo.

echo Step 5: Seeding accounts and transactions...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%~dp0seed-03-accounts-transactions.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to seed accounts and transactions
    pause
    exit /b 1
)
echo Accounts and transactions seeded successfully!
echo.

echo Step 6: Seeding loan products and loans...
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%~dp0seed-04-loans.sql"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to seed loans
    pause
    exit /b 1
)
echo Loans seeded successfully!
echo.

echo ========================================
echo Database setup and seeding completed!
echo ========================================
echo.
echo Database: %DATABASE%
echo Server: %SERVER%
echo.
echo You can now start the application with: npm run dev
echo.
pause
