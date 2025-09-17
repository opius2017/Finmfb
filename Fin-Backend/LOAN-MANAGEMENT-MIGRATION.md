# Loan Management System Database Migration

## Overview

This document provides instructions for creating and applying the database migrations for the new Loan Management System.

## Prerequisites

- .NET Core SDK installed
- Entity Framework Core tools installed
- Access to the application database

## Step 1: Install the EF Core Tools

If you haven't already installed the Entity Framework Core tools, you can do so with the following command:

```bash
dotnet tool install --global dotnet-ef
```

Or update the tools if already installed:

```bash
dotnet tool update --global dotnet-ef
```

## Step 2: Apply the Migrations

The migrations have been created with the following files:
- `20250917000000_LoanManagementSystem.cs` - Creates all tables and relationships for the loan management system
- `20250917000001_SeedLoanData.cs` - Seeds initial data for loan products and fee types

To apply these migrations to your database, run the following command:

```bash
dotnet ef database update --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext
```

## Step 3: Verify the Database Schema

After applying the migration, verify that the following tables have been created in the database under the `loans` schema:

- `loans.LoanProducts`
- `loans.LoanApplications`
- `loans.Loans`
- `loans.LoanTransactions`
- `loans.LoanRepaymentSchedules`
- `loans.LoanCollaterals`
- `loans.LoanGuarantors`
- `loans.LoanDocuments`
- `loans.LoanCollections`
- `loans.LoanFees`
- `loans.LoanCreditChecks`

## Step 4: Database Structure

### Schema: loans

The loan management system uses a dedicated schema `loans` to organize all related tables.

### Key Tables and Relationships:

1. **LoanProducts**
   - Defines available loan products with terms and conditions
   - Primary configuration for interest rates, terms, and eligibility

2. **LoanApplications**
   - Records customer applications for loans
   - Tracks application status through approval workflow

3. **Loans**
   - Core entity representing active loans
   - Links to customer, product, and application
   - Tracks loan status and repayment progress

4. **LoanRepaymentSchedules**
   - Defines installment schedule for each loan
   - Tracks paid and outstanding amounts

5. **LoanTransactions**
   - Records all financial transactions for loans
   - Includes disbursements, repayments, fees, etc.

6. **LoanCollaterals**
   - Tracks assets provided as security for loans
   - Includes valuation and approval status

7. **LoanGuarantors**
   - Records third-party guarantors for loans
   - Tracks approval status and relationship

8. **LoanDocuments**
   - Manages documents attached to loan applications and active loans
   - Includes verification workflow

9. **LoanCollections**
   - Manages recovery actions for overdue loans
   - Tracks collection agents and follow-up activities

10. **LoanFees**
    - Defines fee structure for loan products
    - Can be percentage-based or fixed amount

11. **LoanCreditChecks**
    - Records credit assessment results
    - Tracks credit bureau inquiries and scores

## Step 5: Seed Data

The migration includes seed data for:

1. Five loan product types:
   - Personal Loan
   - Business Loan
   - Micro Loan
   - Mortgage Loan
   - Agricultural Loan

2. Five standard fee types:
   - Processing Fee
   - Credit Life Insurance
   - Administrative Fee
   - Late Payment Penalty
   - Early Settlement Fee

## Troubleshooting

If you encounter issues when applying the migrations, try the following steps:

1. Ensure your database connection string is correctly configured in appsettings.json
2. Verify that you have sufficient permissions to create tables and schemas
3. Check for existing tables that might conflict with the new schema
4. Run with verbose output for more information:

```bash
dotnet ef database update --verbose --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext
```

For more assistance, contact the development team.