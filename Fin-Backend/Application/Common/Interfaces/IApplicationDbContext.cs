using Microsoft.EntityFrameworkCore;
using FinTech.Domain.Entities.GeneralLedger;
using FinTech.Domain.Entities.Identity;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Entities.Deposits;
using FinTech.Domain.Entities.Loans;
using FinTech.Domain.Entities.AccountsPayable;
using FinTech.Domain.Entities.AccountsReceivable;
using FinTech.Domain.Entities.Inventory;
using FinTech.Domain.Entities.Payroll;
using FinTech.Domain.Entities.Security;
using FinTech.Domain.Entities.Reporting;
using FinTech.Domain.Entities.MultiCurrency;

namespace FinTech.Core.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // General Ledger
    DbSet<ChartOfAccounts> ChartOfAccounts { get; }
    DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryDetail> JournalEntryDetails { get; }

    // Identity & Multi-tenancy
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantModule> TenantModules { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<UserRole> UserRoles { get; }


    // Customer Management
    DbSet<Customer> Customers { get; }

    // Deposit Management
    DbSet<DepositProduct> DepositProducts { get; }
    DbSet<DepositAccount> DepositAccounts { get; }
    DbSet<DepositTransaction> DepositTransactions { get; }

    // Loan Management
    DbSet<LoanProduct> LoanProducts { get; }
    DbSet<LoanAccount> LoanAccounts { get; }
    DbSet<LoanTransaction> LoanTransactions { get; }
    DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; }
    DbSet<LoanCollateral> LoanCollaterals { get; }
    DbSet<LoanGuarantor> LoanGuarantors { get; }

    // Accounts Payable
    DbSet<Vendor> Vendors { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
    DbSet<VendorBill> VendorBills { get; }
    DbSet<VendorBillItem> VendorBillItems { get; }
    DbSet<VendorPayment> VendorPayments { get; }

    // Accounts Receivable
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<CustomerPayment> CustomerPayments { get; }

    // Inventory Management
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<StockAdjustment> StockAdjustments { get; }

    // Payroll & HR
    DbSet<Employee> Employees { get; }
    DbSet<PayrollEntry> PayrollEntries { get; }

    // Security & Audit
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<MakerCheckerTransaction> MakerCheckerTransactions { get; }

    // Reporting
    DbSet<FinancialStatement> FinancialStatements { get; }
    DbSet<RegulatoryReport> RegulatoryReports { get; }

    // Multi-Currency
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<CurrencyRevaluation> CurrencyRevaluations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}