using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
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
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Domain.Entities.RegulatoryReporting;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Infrastructure.Data.Configuration;
using FinTech.WebAPI.Domain.Entities.Auth;

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
    public new DbSet<UserRole> UserRoles { get; set; }
    
    // Customer Management
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerDocument> CustomerDocuments { get; set; }
    public DbSet<CustomerNextOfKin> CustomerNextOfKins { get; set; }
    public DbSet<CustomerInquiry> CustomerInquiries { get; set; }
    public DbSet<CustomerComplaint> CustomerComplaints { get; set; }
    public DbSet<CustomerCommunicationLog> CustomerCommunicationLogs { get; set; }

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
    
    // Fixed Assets
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<AssetDepreciationSchedule> AssetDepreciationSchedules { get; set; }
    public DbSet<AssetMaintenance> AssetMaintenances { get; set; }
    public DbSet<AssetRevaluation> AssetRevaluations { get; set; }
    public DbSet<AssetTransfer> AssetTransfers { get; set; }
    public DbSet<AssetInventoryCount> AssetInventoryCounts { get; set; }
    public DbSet<AssetInventoryCountItem> AssetInventoryCountItems { get; set; }
    public DbSet<AssetDisposal> AssetDisposals { get; set; }

    // Regulatory Reporting
    public DbSet<RegulatoryReportTemplate> RegulatoryReportTemplates { get; set; }
    public DbSet<RegulatoryReportSection> RegulatoryReportSections { get; set; }
    public DbSet<RegulatoryReportField> RegulatoryReportFields { get; set; }
    public DbSet<RegulatoryReportSubmission> RegulatoryReportSubmissions { get; set; }
    public DbSet<RegulatoryReportData> RegulatoryReportData { get; set; }
    public DbSet<RegulatoryReportValidation> RegulatoryReportValidations { get; set; }
    public DbSet<RegulatoryReportSchedule> RegulatoryReportSchedules { get; set; }

    // MFA and Security
    public DbSet<UserMfaSettings> UserMfaSettings { get; set; }
    public DbSet<MfaBackupCode> MfaBackupCodes { get; set; }
    public DbSet<MfaChallenge> MfaChallenges { get; set; }
    public DbSet<TrustedDevice> TrustedDevices { get; set; }
    public DbSet<SecurityActivity> SecurityActivities { get; set; }
    public DbSet<SecurityPreferences> SecurityPreferences { get; set; }

    // Client Portal
    public DbSet<ClientPortalProfile> ClientPortalProfiles { get; set; }
    public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
    public DbSet<DashboardPreferences> DashboardPreferences { get; set; }
    public DbSet<ClientPortalSession> ClientPortalSessions { get; set; }
    public DbSet<ClientPortalActivity> ClientPortalActivities { get; set; }
    public DbSet<SavedPayee> SavedPayees { get; set; }
    public DbSet<SavedTransferTemplate> SavedTransferTemplates { get; set; }
    public DbSet<ClientDocument> ClientDocuments { get; set; }
    public DbSet<ClientSupportTicket> ClientSupportTickets { get; set; }
    public DbSet<ClientSupportMessage> ClientSupportMessages { get; set; }
    public DbSet<ClientSupportAttachment> ClientSupportAttachments { get; set; }
    public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
    public DbSet<FrequentlyAskedQuestion> FrequentlyAskedQuestions { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<SavingsGoalTransaction> SavingsGoalTransactions { get; set; }
    public DbSet<ClientNotification> ClientNotifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<NotificationDeliveryRecord> NotificationDeliveryRecords { get; set; }
    public DbSet<ClientSession> ClientSessions { get; set; }
    public DbSet<ClientDevice> ClientDevices { get; set; }
    
    // Payments and Transfers
    public DbSet<Biller> Billers { get; set; }
    public DbSet<BillPayment> BillPayments { get; set; }
    public DbSet<ExternalTransfer> ExternalTransfers { get; set; }
    public DbSet<RecurringPayment> RecurringPayments { get; set; }
    public DbSet<RecurringPaymentHistory> RecurringPaymentHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from assembly
        builder.ApplyAllConfigurations();
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