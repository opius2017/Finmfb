using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.GeneralLedger;
using GLJournalEntry = FinTech.Core.Domain.Entities.GeneralLedger.JournalEntry;
using FinTech.Core.Domain.Entities.Identity;
using IdentityRefreshToken = FinTech.Core.Domain.Entities.Identity.RefreshToken;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.AccountsPayable;
using APVendor = FinTech.Core.Domain.Entities.AccountsPayable.Vendor;
using FinTech.Core.Domain.Entities.AccountsReceivable;
using FinTech.Core.Domain.Entities.Inventory;
using FinTech.Core.Domain.Entities.Payroll;
using FinTech.Core.Domain.Entities.Security;
using FinTech.Core.Domain.Entities.Reporting;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using RegulatoryReport = FinTech.Core.Domain.Entities.RegulatoryReporting.RegulatoryReport;
using FinTech.Core.Domain.Entities.Currency;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Domain.Entities.Tax;

namespace FinTech.Core.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // General Ledger - Using FinTech.Core.Domain.Entities.GeneralLedger.JournalEntry
    DbSet<ChartOfAccounts> ChartOfAccounts { get; }
    DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; }
    DbSet<GLJournalEntry> JournalEntries { get; }
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
    DbSet<LoanDocument> LoanDocuments { get; }
    DbSet<LoanFee> LoanFees { get; }
    DbSet<LoanCollection> LoanCollections { get; }
    DbSet<LoanCreditCheck> LoanCreditChecks { get; }

    // Accounts Payable - Using APVendor alias for AccountsPayable.Vendor
    DbSet<APVendor> Vendors { get; }
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
    DbSet<RegulatoryReportSubmission> RegulatoryReportSubmissions { get; }
    DbSet<RegulatoryReportData> RegulatoryReportData { get; }
    DbSet<RegulatoryReportValidation> RegulatoryReportValidations { get; }
    DbSet<RegulatoryReportSchedule> RegulatoryReportSchedules { get; }

    // Multi-Currency
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<CurrencyRevaluation> CurrencyRevaluations { get; }
    DbSet<FinTech.Core.Domain.Entities.Currency.Currency> Currencies { get; } // Added
    
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
    DbSet<ClientSession> ClientSessions { get; }
    DbSet<ClientDevice> ClientDevices { get; }
    DbSet<Biller> Billers { get; }
    DbSet<BillPayment> BillPayments { get; }
    DbSet<ExternalTransfer> ExternalTransfers { get; }
    DbSet<RecurringPayment> RecurringPayments { get; }
    DbSet<RecurringPaymentHistory> RecurringPaymentHistory { get; }

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
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<NotificationDeliveryRecord> NotificationDeliveryRecords { get; }
    DbSet<LoanApplicationRequest> LoanApplicationRequests { get; }

    // FinTech Best Practice: Knowledge Base and FAQ support
    DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; }
    DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; }
    DbSet<FrequentlyAskedQuestion> FrequentlyAskedQuestions { get; }


    // Tax
    DbSet<TaxTransaction> TaxTransactions { get; }
    DbSet<TaxType> TaxTypes { get; }
    DbSet<TaxRate> TaxRates { get; }
    DbSet<TaxExemption> TaxExemptions { get; }

    // Regulatory Reporting Templates
    DbSet<RegulatoryReportTemplate> RegulatoryReportTemplates { get; }
    DbSet<RegulatoryReportSection> RegulatoryReportSections { get; }
    DbSet<RegulatoryReportField> RegulatoryReportFields { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
