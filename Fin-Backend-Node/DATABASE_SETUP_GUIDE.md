# Database Setup Guide

## Quick Setup (Recommended)

### Option 1: Using Command Prompt (CMD)
```cmd
cd Fin-Backend-Node\scripts
setup-and-seed-database.cmd
```

### Option 2: Using PowerShell
```powershell
cd Fin-Backend-Node\scripts
.\setup-and-seed-database.ps1
```

This will automatically:
1. Create the `SoarMFBDb` database
2. Create all tables and indexes
3. Seed with Nigerian sample data
4. Set up users, roles, and permissions

## Manual Setup

If you prefer to run scripts individually:

### Step 1: Create Database
```cmd
sqlcmd -S localhost -E -i scripts\create-database.sql
```

### Step 2: Create Schema
```cmd
sqlcmd -S localhost -d SoarMFBDb -E -i scripts\create-schema.sql
```

### Step 3: Seed Data
```cmd
sqlcmd -S localhost -d SoarMFBDb -E -i scripts\seed-01-users-roles.sql
sqlcmd -S localhost -d SoarMFBDb -E -i scripts\seed-02-branches-members.sql
sqlcmd -S localhost -d SoarMFBDb -E -i scripts\seed-03-accounts-transactions.sql
sqlcmd -S localhost -d SoarMFBDb -E -i scripts\seed-04-loans.sql
```

## Connection String

Update your `.env` file with:
```
DATABASE_URL="Server=localhost;Database=SoarMFBDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## Sample Data Included

### Users (8 users)
- **Admin**: admin@soarmfb.ng (Super Admin)
- **Branch Managers**: manager.lagos@soarmfb.ng, manager.abuja@soarmfb.ng
- **Loan Officer**: loanofficer@soarmfb.ng
- **Accountant**: accountant@soarmfb.ng
- **Compliance Officer**: compliance@soarmfb.ng
- **Tellers**: teller1@soarmfb.ng, teller2@soarmfb.ng

**Default Password for all users**: `Password123!`

### Branches (5 branches)
- Lagos Island Branch (LIS001)
- Ikeja Branch (IKJ002)
- Abuja Central Branch (ABJ003)
- Port Harcourt Branch (PHC004)
- Kano Branch (KAN005)

### Members (20 members)
- Distributed across all branches
- Nigerian names and addresses
- Active status with valid contact information

### Accounts
- Savings accounts for all members
- Shares accounts for selected members
- Realistic balances (₦50,000 - ₦500,000)

### Transactions
- Sample deposits and withdrawals
- Completed transactions with references
- Date range: Last 30 days

### Loan Products (5 products)
- Personal Loan (18% interest)
- Business Loan (15% interest)
- Agricultural Loan (12% interest)
- Education Loan (10% interest)
- Emergency Loan (20% interest)

### Loans
- 3 active loans with repayment schedules
- 2 pending loan applications
- Realistic amounts and terms

## Verification

After setup, verify the database:

```sql
-- Check tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check user count
SELECT COUNT(*) as UserCount FROM users;

-- Check member count
SELECT COUNT(*) as MemberCount FROM members;

-- Check account count
SELECT COUNT(*) as AccountCount FROM accounts;

-- Check loan count
SELECT COUNT(*) as LoanCount FROM loans;
```

Expected results:
- 30+ tables
- 8 users
- 20 members
- 25+ accounts
- 5 loans

## Troubleshooting

### Error: "Login failed for user"
- Ensure SQL Server is running
- Verify Windows Authentication is enabled
- Check if your Windows user has access to SQL Server

### Error: "Database already exists"
- The script will drop and recreate the database
- Ensure no active connections to the database

### Error: "sqlcmd is not recognized"
- Install SQL Server Command Line Tools
- Add SQL Server bin folder to PATH

### Error: "Cannot open database"
- Verify SQL Server service is running
- Check SQL Server Configuration Manager

## Next Steps

1. **Update .env file** with the connection string
2. **Start the application**: `npm run dev`
3. **Test login** with any of the seeded users
4. **Explore the API** at http://localhost:3000/api/docs

## Database Schema

The database includes the following main tables:

### Authentication & Authorization
- users
- roles
- permissions
- user_roles
- role_permissions
- sessions

### Core Banking
- branches
- members
- accounts
- transactions

### Loans
- loan_products
- loans
- loan_schedules
- loan_payments
- guarantors

### Financial Management
- budgets
- budget_items
- budget_actuals

### Document Management
- documents
- document_versions

### Bank Integration
- bank_connections
- bank_transactions

### Workflow
- approval_requests
- approvals

### Regulatory Compliance
- regulatory_reports
- compliance_checklists
- regulatory_alerts
- tax_calculations
- ecl_provisions

### Audit & Logging
- audit_logs
- system_logs

## Support

For issues or questions:
1. Check the error message in the console
2. Review SQL Server error logs
3. Verify connection string in .env file
4. Ensure all prerequisites are installed
