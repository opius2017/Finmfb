# Database Setup Guide - FinMFB

This guide will help you set up the SQL Server database for the FinMFB application with Nigerian sample data.

## Prerequisites

1. **SQL Server** installed and running (SQL Server 2019 or later recommended)
2. **Node.js** (v20 or later)
3. **npm** (v10 or later)

## Quick Setup (Automated)

### Option 1: Using PowerShell Script (Recommended)

**For Windows Authentication (No Password Required):**

1. Open PowerShell as Administrator
2. Navigate to the project directory:
   ```powershell
   cd Fin-Backend-Node
   ```

3. Run the setup script:
   ```powershell
   .\scripts\setup-database.ps1
   ```

The script will automatically:
- Connect using your Windows credentials
- Create the database
- Run migrations
- Seed sample data

### Option 2: Using Batch Script (Alternative)

1. Right-click `scripts\setup-database.cmd`
2. Select "Run as Administrator"
3. Follow the on-screen prompts

The script will:
- Create the `FinMFBDb` database
- Update your `.env` file with the connection string
- Run Prisma migrations
- Seed the database with Nigerian sample data

### Option 2: Manual Setup

#### Step 1: Create Database

Connect to SQL Server and run:

```sql
CREATE DATABASE FinMFBDb
COLLATE Latin1_General_100_CI_AS_SC_UTF8;
GO
```

#### Step 2: Configure Environment

1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```

2. Update the `DATABASE_URL` in `.env`:
   
   **For Windows Authentication (Recommended for local development):**
   ```
   DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;integratedSecurity=true;trustServerCertificate=true;encrypt=true"
   ```

   **For SQL Server Authentication (if needed):**
   ```
   DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;user=sa;password=YourPassword123;trustServerCertificate=true;encrypt=true"
   ```
   Replace `YourPassword123` with your actual SQL Server password.

#### Step 3: Install Dependencies

```bash
npm install
```

#### Step 4: Generate Prisma Client

```bash
npx prisma generate
```

#### Step 5: Push Database Schema

```bash
npx prisma db push
```

#### Step 6: Seed Database

```bash
npm run db:seed
```

## Database Connection String Format

### Windows Authentication (Recommended for Local Development)

**Format:**
```
sqlserver://[HOST]:[PORT];database=[DATABASE];integratedSecurity=true;trustServerCertificate=true;encrypt=true
```

**Example:**
```
sqlserver://localhost:1433;database=FinMFBDb;integratedSecurity=true;trustServerCertificate=true;encrypt=true
```

**Benefits:**
- ✅ No password required
- ✅ Uses your Windows credentials
- ✅ More secure for local development
- ✅ Easier to set up

### SQL Server Authentication

**Format:**
```
sqlserver://[HOST]:[PORT];database=[DATABASE];user=[USERNAME];password=[PASSWORD];trustServerCertificate=true;encrypt=true
```

**Examples:**

**Local SQL Server with SA account:**
```
sqlserver://localhost:1433;database=FinMFBDb;user=sa;password=YourPassword;trustServerCertificate=true;encrypt=true
```

**Remote SQL Server:**
```
sqlserver://192.168.1.100:1433;database=FinMFBDb;user=finmfb_user;password=SecurePassword;trustServerCertificate=true;encrypt=true
```

## Seeded Data Overview

The seed script creates the following Nigerian-themed data:

### Branches (5)
- Lagos Head Office (Lagos State)
- Abuja Branch (FCT)
- Port Harcourt Branch (Rivers State)
- Kano Branch (Kano State)
- Ibadan Branch (Oyo State)

### Users (5)
| Email | Role | Name | Password |
|-------|------|------|----------|
| admin@finmfb.ng | Admin | Chukwuemeka Okonkwo | Password123! |
| manager@finmfb.ng | Branch Manager | Aisha Bello | Password123! |
| teller@finmfb.ng | Teller | Ngozi Eze | Password123! |
| loanofficer@finmfb.ng | Loan Officer | Oluwaseun Adeyemi | Password123! |
| accountant@finmfb.ng | Accountant | Ibrahim Musa | Password123! |

### Members (20)
Nigerian members with authentic names from various states including:
- Adebayo Ogunleye (Lagos)
- Chidinma Nwosu (Enugu)
- Fatima Abdullahi (Kano)
- Emeka Okafor (Abia)
- And 16 more...

### Accounts
- 20+ Savings accounts
- 10+ Shares accounts
- Balances ranging from ₦10,000 to ₦500,000

### Transactions
- 50+ sample transactions
- Deposits, withdrawals, transfers
- Interest payments and loan repayments

### Loan Products (5)
1. **Personal Loan** - 15% interest, ₦50K - ₦500K, 3-12 months
2. **Business Loan** - 18% interest, ₦100K - ₦2M, 6-24 months
3. **Emergency Loan** - 20% interest, ₦20K - ₦200K, 1-6 months
4. **Asset Finance** - 16% interest, ₦500K - ₦5M, 12-36 months
5. **Salary Advance** - 12% interest, ₦30K - ₦300K, 1-3 months

### Loans (15)
- Various statuses: Pending, Approved, Disbursed, Active, Closed
- Different loan products and amounts
- Realistic Nigerian business purposes

### Budget
- 2024 Annual Budget (₦50M)
- 10 budget categories including:
  - Staff Salaries (₦20M)
  - Office Rent (₦5M)
  - Marketing & Advertising (₦3M)
  - IT Infrastructure (₦4M)
  - And more...

### Bank Connections (3)
- Access Bank
- GTBank
- First Bank

## Useful Commands

### View Database in Prisma Studio
```bash
npm run db:studio
```
Opens a web interface at http://localhost:5555 to browse your database.

### Reset Database
```bash
npx prisma migrate reset
```
⚠️ This will delete all data and recreate the database.

### Re-seed Database
```bash
npm run db:seed
```

### Generate Prisma Client
```bash
npx prisma generate
```

### Push Schema Changes
```bash
npx prisma db push
```

## Troubleshooting

### Connection Issues

**Error: "Login failed for user 'sa'"**
- Verify SQL Server is running
- Check that SQL Server Authentication is enabled
- Verify the password is correct

**Error: "A network-related or instance-specific error"**
- Ensure SQL Server is running
- Check that TCP/IP is enabled in SQL Server Configuration Manager
- Verify the port (default 1433) is correct
- Check firewall settings

### Enable SQL Server Authentication

1. Open SQL Server Management Studio (SSMS)
2. Right-click on the server → Properties
3. Go to Security page
4. Select "SQL Server and Windows Authentication mode"
5. Restart SQL Server service

### Enable TCP/IP

1. Open SQL Server Configuration Manager
2. Expand "SQL Server Network Configuration"
3. Click "Protocols for [Your Instance]"
4. Right-click "TCP/IP" → Enable
5. Restart SQL Server service

### Check SQL Server Service

```powershell
# Check if SQL Server is running
Get-Service -Name MSSQLSERVER

# Start SQL Server if stopped
Start-Service -Name MSSQLSERVER
```

## Database Schema

The database includes the following main tables:

- **Authentication**: users, roles, permissions, sessions
- **Members**: members, accounts, transactions
- **Loans**: loan_products, loans, loan_schedules, loan_payments, guarantors
- **Budgets**: budgets, budget_items, budget_actuals
- **Documents**: documents, document_versions
- **Banking**: bank_connections, bank_transactions
- **Approvals**: approval_requests, approvals
- **Audit**: audit_logs, system_logs

## Next Steps

After setting up the database:

1. Start the development server:
   ```bash
   npm run dev
   ```

2. Access the API documentation:
   ```
   http://localhost:3000/api-docs
   ```

3. Login with one of the default accounts to test the system

4. Explore the data using Prisma Studio:
   ```bash
   npm run db:studio
   ```

## Support

For issues or questions:
- Check the main README.md
- Review Prisma documentation: https://www.prisma.io/docs
- Check SQL Server documentation: https://docs.microsoft.com/sql

## Security Notes

⚠️ **Important**: 
- Change all default passwords before deploying to production
- Use environment variables for sensitive data
- Never commit `.env` file to version control
- Use strong passwords for SQL Server accounts
- Enable SSL/TLS for production databases
- Regularly backup your database
