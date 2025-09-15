using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Entities.Identity;
using FinTech.Domain.Entities.GeneralLedger;
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

namespace FinTech.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, FinTech.Core.Application.Common.Interfaces.IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // General Ledger
    public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalEntryDetail> JournalEntryDetails { get; set; }

    // Identity & Multi-tenancy
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantModule> TenantModules { get; set; }
    public new DbSet<IdentityRole<Guid>> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<ModuleDashboard> ModuleDashboards { get; set; }
    public DbSet<UserDashboardPreference> UserDashboardPreferences { get; set; }
        public new DbSet<UserRole> UserRoles { get; set; }    // Customer Management
    public DbSet<Customer> Customers { get; set; }

    // Deposit Management
    public DbSet<DepositProduct> DepositProducts { get; set; }
    public DbSet<DepositAccount> DepositAccounts { get; set; }
    public DbSet<DepositTransaction> DepositTransactions { get; set; }

    // Loan Management
    public DbSet<LoanProduct> LoanProducts { get; set; }
    public DbSet<LoanAccount> LoanAccounts { get; set; }
    public DbSet<LoanTransaction> LoanTransactions { get; set; }
    public DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; }
    public DbSet<LoanCollateral> LoanCollaterals { get; set; }
    public DbSet<LoanGuarantor> LoanGuarantors { get; set; }

    // Accounts Payable
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<VendorBill> VendorBills { get; set; }
    public DbSet<VendorBillItem> VendorBillItems { get; set; }
    public DbSet<VendorPayment> VendorPayments { get; set; }

    // Accounts Receivable
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<CustomerPayment> CustomerPayments { get; set; }

    // Inventory Management
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }

    // Payroll & HR
    public DbSet<Employee> Employees { get; set; }
    public DbSet<PayrollEntry> PayrollEntries { get; set; }

    // Security & Audit
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<MakerCheckerTransaction> MakerCheckerTransactions { get; set; }

    // Reporting
    public DbSet<FinancialStatement> FinancialStatements { get; set; }
    public DbSet<RegulatoryReport> RegulatoryReports { get; set; }

    // Multi-Currency
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<CurrencyRevaluation> CurrencyRevaluations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entity relationships and constraints
        ConfigureIdentityTables(builder);
        ConfigureGeneralLedger(builder);
        ConfigureCustomers(builder);
        ConfigureDeposits(builder);
        ConfigureLoans(builder);
        ConfigureAccountsPayable(builder);
        ConfigureAccountsReceivable(builder);
        ConfigureInventory(builder);
        ConfigurePayroll(builder);
        ConfigureSecurity(builder);
        ConfigureReporting(builder);
        ConfigureMultiCurrency(builder);
        ConfigureRolePermissions(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        // Tenant configuration
        builder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
        });

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TenantModule configuration
        builder.Entity<TenantModule>(entity =>
        {
            entity.HasOne(tm => tm.Tenant)
                  .WithMany(t => t.TenantModules)
                  .HasForeignKey(tm => tm.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Module }).IsUnique();
        });

        // UserRole configuration
        builder.Entity<UserRole>(entity =>
        {
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRolePermissions(ModelBuilder builder)
    {
        // Role configuration
        builder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.RoleName }).IsUnique();
        });

        // Permission configuration
        builder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => e.PermissionName).IsUnique();
            entity.HasIndex(e => new { e.Module, e.Resource, e.Action }).IsUnique();
        });

        // RolePermission configuration
        builder.Entity<RolePermission>(entity =>
        {
            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });

        // ModuleDashboard configuration
        builder.Entity<ModuleDashboard>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ModuleName, e.DashboardName }).IsUnique();
        });

        // UserDashboardPreference configuration
        builder.Entity<UserDashboardPreference>(entity =>
        {
            entity.HasOne(udp => udp.User)
                  .WithMany()
                  .HasForeignKey(udp => udp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(udp => udp.ModuleDashboard)
                  .WithMany(md => md.UserPreferences)
                  .HasForeignKey(udp => udp.ModuleDashboardId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ModuleDashboardId }).IsUnique();
        });
    }

    private static void ConfigureGeneralLedger(ModelBuilder builder)
    {
        // Chart of Accounts configuration
        builder.Entity<ChartOfAccounts>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.AccountCode }).IsUnique();
            entity.HasOne(c => c.ParentAccount)
                  .WithMany(c => c.SubAccounts)
                  .HasForeignKey(c => c.ParentAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // General Ledger Entry configuration
        builder.Entity<GeneralLedgerEntry>(entity =>
        {
            entity.HasOne(g => g.Account)
                  .WithMany(a => a.GeneralLedgerEntries)
                  .HasForeignKey(g => g.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TransactionReference);
            entity.HasIndex(e => e.TransactionDate);
        });

        // Journal Entry configuration
        builder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(e => e.JournalNumber).IsUnique();
            entity.HasMany(j => j.Details)
                  .WithOne(d => d.JournalEntry)
                  .HasForeignKey(d => d.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Journal Entry Detail configuration
        builder.Entity<JournalEntryDetail>(entity =>
        {
            entity.HasOne(d => d.Account)
                  .WithMany()
                  .HasForeignKey(d => d.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCustomers(ModelBuilder builder)
    {
        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.CustomerNumber }).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.BVN);
            entity.HasIndex(e => e.NIN);
        });
    }

    private static void ConfigureDeposits(ModelBuilder builder)
    {
        // Deposit Product configuration
        builder.Entity<DepositProduct>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ProductCode }).IsUnique();
        });

        // Deposit Account configuration
        builder.Entity<DepositAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasOne(a => a.Customer)
                  .WithMany()
                  .HasForeignKey(a => a.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Product)
                  .WithMany(p => p.DepositAccounts)
                  .HasForeignKey(a => a.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Deposit Transaction configuration
        builder.Entity<DepositTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionReference).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLoans(ModelBuilder builder)
    {
        // Loan Product configuration
        builder.Entity<LoanProduct>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ProductCode }).IsUnique();
        });

        // Loan Account configuration
        builder.Entity<LoanAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasOne(a => a.Customer)
                  .WithMany()
                  .HasForeignKey(a => a.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Product)
                  .WithMany(p => p.LoanAccounts)
                  .HasForeignKey(a => a.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Loan Transaction configuration
        builder.Entity<LoanTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionReference).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasOne(t => t.LoanAccount)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.LoanAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Loan Repayment Schedule configuration
        builder.Entity<LoanRepaymentSchedule>(entity =>
        {
            entity.HasIndex(e => new { e.LoanAccountId, e.InstallmentNumber }).IsUnique();
            entity.HasOne(s => s.LoanAccount)
                  .WithMany(a => a.RepaymentSchedule)
                  .HasForeignKey(s => s.LoanAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Loan Collateral configuration
        builder.Entity<LoanCollateral>(entity =>
        {
            entity.HasOne(c => c.LoanAccount)
                  .WithMany(a => a.Collaterals)
                  .HasForeignKey(c => c.LoanAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Loan Guarantor configuration
        builder.Entity<LoanGuarantor>(entity =>
        {
            entity.HasOne(g => g.LoanAccount)
                  .WithMany(a => a.Guarantors)
                  .HasForeignKey(g => g.LoanAccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Customer)
                  .WithMany()
                  .HasForeignKey(g => g.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureAccountsPayable(ModelBuilder builder)
    {
        // Vendor configuration
        builder.Entity<Vendor>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.VendorNumber }).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.TINNumber);
        });

        // Purchase Order configuration
        builder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(e => e.PONumber).IsUnique();
            entity.HasOne(p => p.Vendor)
                  .WithMany(v => v.PurchaseOrders)
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Purchase Order Item configuration
        builder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasOne(i => i.PurchaseOrder)
                  .WithMany(p => p.Items)
                  .HasForeignKey(i => i.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vendor Bill configuration
        builder.Entity<VendorBill>(entity =>
        {
            entity.HasIndex(e => e.BillNumber).IsUnique();
            entity.HasOne(b => b.Vendor)
                  .WithMany(v => v.VendorBills)
                  .HasForeignKey(b => b.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.PurchaseOrder)
                  .WithMany()
                  .HasForeignKey(b => b.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Vendor Bill Item configuration
        builder.Entity<VendorBillItem>(entity =>
        {
            entity.HasOne(i => i.VendorBill)
                  .WithMany(b => b.Items)
                  .HasForeignKey(i => i.VendorBillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vendor Payment configuration
        builder.Entity<VendorPayment>(entity =>
        {
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.VendorBill)
                  .WithMany(b => b.Payments)
                  .HasForeignKey(p => p.VendorBillId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAccountsReceivable(ModelBuilder builder)
    {
        // Invoice configuration
        builder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasOne(i => i.Customer)
                  .WithMany()
                  .HasForeignKey(i => i.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice Item configuration
        builder.Entity<InvoiceItem>(entity =>
        {
            entity.HasOne(i => i.Invoice)
                  .WithMany(inv => inv.Items)
                  .HasForeignKey(i => i.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer Payment configuration
        builder.Entity<CustomerPayment>(entity =>
        {
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasOne(p => p.Customer)
                  .WithMany()
                  .HasForeignKey(p => p.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(p => p.InvoiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInventory(ModelBuilder builder)
    {
        // Inventory Item configuration
        builder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ItemCode }).IsUnique();
            entity.HasIndex(e => e.SKU);
            entity.HasIndex(e => e.Barcode);
        });

        // Inventory Transaction configuration
        builder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasOne(t => t.InventoryItem)
                  .WithMany(i => i.Transactions)
                  .HasForeignKey(t => t.InventoryItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Stock Adjustment configuration
        builder.Entity<StockAdjustment>(entity =>
        {
            entity.HasIndex(e => e.AdjustmentNumber).IsUnique();
            entity.HasOne(a => a.InventoryItem)
                  .WithMany(i => i.StockAdjustments)
                  .HasForeignKey(a => a.InventoryItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayroll(ModelBuilder builder)
    {
        // Employee configuration
        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.EmployeeNumber }).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.BVN);
            entity.HasIndex(e => e.NIN);
        });

        // Payroll Entry configuration
        builder.Entity<PayrollEntry>(entity =>
        {
            entity.HasIndex(e => e.PayrollNumber).IsUnique();
            entity.HasIndex(e => new { e.EmployeeId, e.PayPeriodStart, e.PayPeriodEnd }).IsUnique();
            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.PayrollEntries)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSecurity(ModelBuilder builder)
    {
        // Audit Log configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.Action);
        });

        // Maker Checker Transaction configuration
        builder.Entity<MakerCheckerTransaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionReference).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.MakerId);
            entity.HasIndex(e => e.CheckerId);
        });
    }

    private static void ConfigureReporting(ModelBuilder builder)
    {
        // Financial Statement configuration
        builder.Entity<FinancialStatement>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.StatementType, e.PeriodStart, e.PeriodEnd }).IsUnique();
        });

        // Regulatory Report configuration
        builder.Entity<RegulatoryReport>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ReportCode, e.ReportingPeriodStart }).IsUnique();
            entity.HasIndex(e => e.DueDate);
        });
    }

    private static void ConfigureMultiCurrency(ModelBuilder builder)
    {
        // Exchange Rate configuration
        builder.Entity<ExchangeRate>(entity =>
        {
            entity.HasIndex(e => new { e.FromCurrency, e.ToCurrency, e.EffectiveDate }).IsUnique();
            entity.HasIndex(e => e.EffectiveDate);
        });

        // Currency Revaluation configuration
        builder.Entity<CurrencyRevaluation>(entity =>
        {
            entity.HasIndex(e => e.RevaluationNumber).IsUnique();
            entity.HasIndex(e => e.RevaluationDate);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}