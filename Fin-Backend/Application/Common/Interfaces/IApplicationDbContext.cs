using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.GeneralLedger;
using FinTech.Core.Domain.Entities.Identity;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.AccountsPayable;
using FinTech.Core.Domain.Entities.AccountsReceivable;
using FinTech.Core.Domain.Entities.Inventory;
using FinTech.Core.Domain.Entities.Payroll;
using FinTech.Core.Domain.Entities.Security;
using FinTech.Core.Domain.Entities.Reporting;
using FinTech.Core.Domain.Entities.MultiCurrency;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Entities.ClientPortal;

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
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<CustomerNextOfKin> CustomerNextOfKins { get; }
    DbSet<CustomerInquiry> CustomerInquiries { get; }
    DbSet<CustomerComplaint> CustomerComplaints { get; }
    DbSet<CustomerCommunicationLog> CustomerCommunicationLogs { get; }

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
    DbSet<Loan> Loans { get; }
    DbSet<LoanApplication> LoanApplications { get; }
    DbSet<LoanDocument> LoanDocuments { get; }
    
    // Loan Governance & Compliance
    DbSet<LoanType> LoanTypes { get; }
    DbSet<LoanConfiguration> LoanConfigurations { get; }
    DbSet<LoanCommitteeApproval> LoanCommitteeApprovals { get; }
    DbSet<LoanEligibilityRule> LoanEligibilityRules { get; }
    DbSet<LoanRegister> LoanRegisters { get; }
    DbSet<CommodityLoan> CommodityLoans { get; }

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
    
    // Fixed Assets
    DbSet<Asset> Assets { get; }
    DbSet<AssetCategory> AssetCategories { get; }
    DbSet<AssetDepreciationSchedule> AssetDepreciationSchedules { get; }
    DbSet<AssetMaintenance> AssetMaintenances { get; }
    DbSet<AssetRevaluation> AssetRevaluations { get; }
    DbSet<AssetTransfer> AssetTransfers { get; }
    DbSet<AssetInventoryCount> AssetInventoryCounts { get; }
    DbSet<AssetInventoryCountItem> AssetInventoryCountItems { get; }
    DbSet<AssetDisposal> AssetDisposals { get; }

    // Client Portal
    DbSet<ClientPortalProfile> ClientPortalProfiles { get; }
    DbSet<NotificationPreferences> NotificationPreferences { get; }
    DbSet<DashboardPreferences> DashboardPreferences { get; }
    DbSet<ClientPortalSession> ClientPortalSessions { get; }
    DbSet<ClientPortalActivity> ClientPortalActivities { get; }
    DbSet<SavedPayee> SavedPayees { get; }
    DbSet<SavedTransferTemplate> SavedTransferTemplates { get; }
    DbSet<ClientDocument> ClientDocuments { get; }
    DbSet<ClientSupportTicket> ClientSupportTickets { get; }
    DbSet<ClientSupportMessage> ClientSupportMessages { get; }
    DbSet<ClientSupportAttachment> ClientSupportAttachments { get; }
    DbSet<SavingsGoal> SavingsGoals { get; }
    DbSet<SavingsGoalTransaction> SavingsGoalTransactions { get; }
    DbSet<ClientNotification> ClientNotifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
