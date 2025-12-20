using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Security;
using FinTech.Core.Domain.Entities.Identity;

using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Entities.GeneralLedger;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Banking;
using FinTech.Core.Domain.Entities.Payroll;
using FinTech.Core.Domain.Entities.Currency;
using FinTech.Core.Domain.Entities.Tax;
using FinTech.Infrastructure.Data.Configuration;
using FinTech.Infrastructure.Data.Configurations.Accounting;
using FinTech.Infrastructure.Data.Interceptors;
using FinTech.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Infrastructure.Data.Auditing;


using FinTech.Infrastructure.Messaging;
using Newtonsoft.Json;

namespace FinTech.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<FinTech.Core.Domain.Entities.Identity.ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        private readonly IDomainEventService? _domainEventService;
        private readonly ILogger<ApplicationDbContext>? _logger;
        private readonly ICurrentUserService? _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IDomainEventService? domainEventService = null,
            ILogger<ApplicationDbContext>? logger = null,
            ICurrentUserService? currentUserService = null) 
            : base(options) 
        {
            _domainEventService = domainEventService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // General Ledger - Legacy
        public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.GeneralLedger.JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryDetail> JournalEntryDetails { get; set; }
        
        // Accounting - New Core Accounting Engine
        public DbSet<ChartOfAccount> CoreChartOfAccounts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.JournalEntry> CoreJournalEntries { get; set; }
        public DbSet<JournalEntryLine> CoreJournalEntryLines { get; set; }
        public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }

        // Identity & Multi-tenancy
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantModule> TenantModules { get; set; }
        public new DbSet<IdentityRole<Guid>> Roles { get; set; }
        public new DbSet<FinTech.Core.Domain.Entities.Identity.ApplicationUser> Users { get; set; }
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
        public DbSet<LoanApplicationRequest> LoanApplicationRequests { get; set; }
        public DbSet<LoanApplicationDocument> LoanApplicationDocuments { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<LoanFee> LoanFees { get; set; }
        public DbSet<LoanCollection> LoanCollections { get; set; }
        public DbSet<LoanCreditCheck> LoanCreditChecks { get; set; }
        
        // Cooperative Loan Management
        public DbSet<Member> Members { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<GuarantorConsent> GuarantorConsents { get; set; }
        public DbSet<CommitteeReview> CommitteeReviews { get; set; }
        public DbSet<LoanRegister> LoanRegisters { get; set; }
        public DbSet<MonthlyThreshold> MonthlyThresholds { get; set; }

        // Accounts Payable
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.Vendor> Vendors { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.VendorBill> VendorBills { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.VendorBillItem> VendorBillItems { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsPayable.VendorPayment> VendorPayments { get; set; }

        // Accounts Receivable
        public DbSet<FinTech.Core.Domain.Entities.AccountsReceivable.Invoice> Invoices { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsReceivable.InvoiceItem> InvoiceItems { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.AccountsReceivable.CustomerPayment> CustomerPayments { get; set; }

        // Inventory Management
        public DbSet<FinTech.Core.Domain.Entities.Inventory.InventoryItem> InventoryItems { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Inventory.InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Inventory.StockAdjustment> StockAdjustments { get; set; }

        // Payroll & HR
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PayrollEntry> PayrollEntries { get; set; }

        // Security & Audit
        public DbSet<FinTech.Core.Domain.Entities.Security.AuditLog> AuditLogs { get; set; }
        public DbSet<MakerCheckerTransaction> MakerCheckerTransactions { get; set; }
        public DbSet<ResourcePermission> ResourcePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<DataAccessLog> DataAccessLogs { get; set; }
        
        // Domain Events Tracking
        public DbSet<FinTech.Core.Domain.Entities.Common.DomainEventRecord> DomainEventRecords { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Common.OutboxMessage> OutboxMessages { get; set; }
        
        // Integration Events Tracking
        public DbSet<FinTech.Infrastructure.Messaging.IntegrationEventOutboxItem> IntegrationEventOutbox { get; set; }

        // Reporting
        public DbSet<FinTech.Core.Domain.Entities.Reporting.FinancialStatement> FinancialStatements { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.RegulatoryReporting.RegulatoryReport> RegulatoryReports { get; set; }

        // Multi-Currency
        public DbSet<FinTech.Core.Domain.Entities.Currency.ExchangeRate> ExchangeRates { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Currency.CurrencyRevaluation> CurrencyRevaluations { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Currency.Currency> Currencies { get; set; }
        
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
        public DbSet<FinTech.Core.Domain.Entities.Identity.SecurityActivity> SecurityActivities { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Identity.UserSecurityPreferences> SecurityPreferences { get; set; }

        // Tax
        // Tax
        public DbSet<TaxTransaction> TaxTransactions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Tax.TaxType> TaxTypes { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Tax.TaxRate> TaxRates { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Tax.TaxExemption> TaxExemptions { get; set; }

        // Client Portal
        public DbSet<ClientPortalProfile> ClientPortalProfiles { get; set; }
        public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
        public DbSet<DashboardPreferences> DashboardPreferences { get; set; }

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
            
            // Apply explicitly the new accounting configurations
            // TODO: Add configuration classes when they exist
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.ChartOfAccountConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.JournalEntryConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.JournalEntryLineConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.FinancialPeriodConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.FiscalYearConfiguration());
            
            // Apply security and authorization configurations
            // TODO: Add security configuration classes when they exist
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.ResourcePermissionConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.UserPermissionConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.SecurityPolicyConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.LoginAttemptConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.DataAccessLogConfiguration());
            
            // Apply event tracking configurations
            // TODO: Add event configuration classes when they exist
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Events.DomainEventRecordConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Messaging.OutboxMessageConfiguration());
            // builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Auditing.AuditLogConfiguration());
            
            // Apply global query filters
            ApplyGlobalFilters(builder);
        }
        
        private void ApplyGlobalFilters(ModelBuilder builder)
        {
            // Apply soft delete filter to all entities implementing ISoftDelete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(FinTech.Core.Domain.Common.ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(FinTech.Core.Domain.Common.ISoftDelete.IsDeleted));
                    var falseValue = Expression.Constant(false);
                    var expression = Expression.Equal(property, falseValue);
                    var lambda = Expression.Lambda(expression, parameter);
                    
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
                
                // Apply multi-tenancy filter if applicable
                if (typeof(FinTech.Core.Domain.Common.ITenantEntity).IsAssignableFrom(entityType.ClrType) && _currentUserService != null)
                {
                    var tenantId = _currentUserService?.TenantId;
                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var property = Expression.Property(parameter, nameof(FinTech.Core.Domain.Common.ITenantEntity.TenantId));
                        var tenantValue = Expression.Constant(tenantId);
                        var expression = Expression.Equal(property, tenantValue);
                        var lambda = Expression.Lambda(expression, parameter);
                        
                        builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Standard EF Core SaveChanges. 
            // Auditing and Domain Events are now handled by Interceptors (AuditSaveChangesInterceptor, DomainEventInterceptor).
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
