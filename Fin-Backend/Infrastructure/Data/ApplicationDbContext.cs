using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Interfaces.Services;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Authentication;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Entities.Banking;
using FinTech.Domain.Entities.Loans;
using FinTech.Domain.Entities.Payroll;
using FinTech.Domain.Entities.Security;
using FinTech.Domain.Entities.Currency;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Domain.Entities.RegulatoryReporting;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Infrastructure.Data.Configuration;
using FinTech.Infrastructure.Data.Configurations.Accounting;
using FinTech.Infrastructure.Data.Interceptors;
using FinTech.Infrastructure.Security.Authorization;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Infrastructure.Data.Auditing;
using FinTech.Infrastructure.Data.Events;
using FinTech.Infrastructure.Data.Messaging;
using FinTech.Infrastructure.Messaging;
using FinTech.Application.Common.Interfaces;
using FinTech.Domain.Entities;
using Newtonsoft.Json;

namespace FinTech.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<FinTech.Domain.Entities.Authentication.ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        private readonly IDomainEventService _domainEventService;
        private readonly ILogger<ApplicationDbContext> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IDomainEventService domainEventService = null,
            ILogger<ApplicationDbContext> logger = null,
            ICurrentUserService currentUserService = null) 
            : base(options) 
        {
            _domainEventService = domainEventService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // General Ledger - Legacy
        public DbSet<FinTech.Domain.Entities.Accounting.ChartOfAccounts> ChartOfAccounts { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.JournalEntry> JournalEntries { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.JournalEntryDetail> JournalEntryDetails { get; set; }
        
        // Accounting - New Core Accounting Engine
        public DbSet<FinTech.Domain.Entities.Accounting.ChartOfAccount> CoreChartOfAccounts { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.JournalEntry> CoreJournalEntries { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.JournalEntryLine> CoreJournalEntryLines { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.FinancialPeriod> FinancialPeriods { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.FiscalYear> FiscalYears { get; set; }

        // Identity & Multi-tenancy
        public DbSet<FinTech.Domain.Entities.Authentication.Tenant> Tenants { get; set; }
        public DbSet<FinTech.Domain.Entities.Authentication.TenantModule> TenantModules { get; set; }
        public new DbSet<IdentityRole<Guid>> Roles { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.Permission> Permissions { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.RolePermission> RolePermissions { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ModuleDashboard> ModuleDashboards { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.UserDashboardPreference> UserDashboardPreferences { get; set; }
        public new DbSet<FinTech.Domain.Entities.Authentication.UserRole> UserRoles { get; set; }
        
        // Customer Management
        public DbSet<FinTech.Domain.Entities.Customers.Customer> Customers { get; set; }
        public DbSet<FinTech.Domain.Entities.Customers.CustomerDocument> CustomerDocuments { get; set; }
        public DbSet<FinTech.Domain.Entities.Customers.CustomerNextOfKin> CustomerNextOfKins { get; set; }
        public DbSet<FinTech.Domain.Entities.Customers.CustomerInquiry> CustomerInquiries { get; set; }
        public DbSet<FinTech.Domain.Entities.Customers.CustomerComplaint> CustomerComplaints { get; set; }
        public DbSet<FinTech.Domain.Entities.Customers.CustomerCommunicationLog> CustomerCommunicationLogs { get; set; }

        // Deposit Management
        public DbSet<FinTech.Domain.Entities.Banking.DepositProduct> DepositProducts { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.DepositAccount> DepositAccounts { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.DepositTransaction> DepositTransactions { get; set; }

        // Loan Management
        public DbSet<FinTech.Domain.Entities.Loans.LoanProduct> LoanProducts { get; set; }
        public DbSet<FinTech.Domain.Entities.Loans.LoanAccount> LoanAccounts { get; set; }
        public DbSet<FinTech.Domain.Entities.Loans.LoanTransaction> LoanTransactions { get; set; }
        public DbSet<FinTech.Domain.Entities.Loans.LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; }
        public DbSet<FinTech.Domain.Entities.Loans.LoanCollateral> LoanCollaterals { get; set; }
        public DbSet<FinTech.Domain.Entities.Loans.LoanGuarantor> LoanGuarantors { get; set; }

        // Accounts Payable
        public DbSet<FinTech.Domain.Entities.AccountsPayable.Vendor> Vendors { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsPayable.PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsPayable.PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsPayable.VendorBill> VendorBills { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsPayable.VendorBillItem> VendorBillItems { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsPayable.VendorPayment> VendorPayments { get; set; }

        // Accounts Receivable
        public DbSet<FinTech.Domain.Entities.AccountsReceivable.Invoice> Invoices { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsReceivable.InvoiceItem> InvoiceItems { get; set; }
        public DbSet<FinTech.Domain.Entities.AccountsReceivable.CustomerPayment> CustomerPayments { get; set; }

        // Inventory Management
        public DbSet<FinTech.Domain.Entities.Inventory.InventoryItem> InventoryItems { get; set; }
        public DbSet<FinTech.Domain.Entities.Inventory.InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<FinTech.Domain.Entities.Inventory.StockAdjustment> StockAdjustments { get; set; }

        // Payroll & HR
        public DbSet<FinTech.Domain.Entities.Payroll.Employee> Employees { get; set; }
        public DbSet<FinTech.Domain.Entities.Payroll.PayrollEntry> PayrollEntries { get; set; }

        // Security & Audit
        public DbSet<FinTech.Domain.Entities.Security.AuditLog> AuditLogs { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.MakerCheckerTransaction> MakerCheckerTransactions { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.ResourcePermission> ResourcePermissions { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.UserPermission> UserPermissions { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.ResourceOperation> ResourceOperations { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.LoginAttempt> LoginAttempts { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.DataAccessLog> DataAccessLogs { get; set; }
        
        // Domain Events Tracking
        public DbSet<FinTech.Infrastructure.Data.Events.DomainEventRecord> DomainEventRecords { get; set; }
        public DbSet<FinTech.Infrastructure.Data.Messaging.OutboxMessage> OutboxMessages { get; set; }
        
        // Integration Events Tracking
        public DbSet<FinTech.Infrastructure.Messaging.IntegrationEventOutboxItem> IntegrationEventOutbox { get; set; }

        // Reporting
        public DbSet<FinTech.Domain.Entities.Reporting.FinancialStatement> FinancialStatements { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReport> RegulatoryReports { get; set; }

        // Multi-Currency
        public DbSet<FinTech.Domain.Entities.Currency.ExchangeRate> ExchangeRates { get; set; }
        public DbSet<FinTech.Domain.Entities.Currency.CurrencyRevaluation> CurrencyRevaluations { get; set; }
        
        // Fixed Assets
        public DbSet<FinTech.Domain.Entities.FixedAssets.Asset> Assets { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetCategory> AssetCategories { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetDepreciationSchedule> AssetDepreciationSchedules { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetMaintenance> AssetMaintenances { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetRevaluation> AssetRevaluations { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetTransfer> AssetTransfers { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetInventoryCount> AssetInventoryCounts { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetInventoryCountItem> AssetInventoryCountItems { get; set; }
        public DbSet<FinTech.Domain.Entities.FixedAssets.AssetDisposal> AssetDisposals { get; set; }

        // Regulatory Reporting
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportTemplate> RegulatoryReportTemplates { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportSection> RegulatoryReportSections { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportField> RegulatoryReportFields { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportSubmission> RegulatoryReportSubmissions { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportData> RegulatoryReportData { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportValidation> RegulatoryReportValidations { get; set; }
        public DbSet<FinTech.Domain.Entities.RegulatoryReporting.RegulatoryReportSchedule> RegulatoryReportSchedules { get; set; }

        // MFA and Security
        public DbSet<FinTech.Domain.Entities.Security.UserMfaSettings> UserMfaSettings { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.MfaBackupCode> MfaBackupCodes { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.MfaChallenge> MfaChallenges { get; set; }
        public DbSet<FinTech.Domain.Entities.Security.TrustedDevice> TrustedDevices { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SecurityActivity> SecurityActivities { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SecurityPreferences> SecurityPreferences { get; set; }

        // Client Portal
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientPortalProfile> ClientPortalProfiles { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.NotificationPreferences> NotificationPreferences { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.DashboardPreferences> DashboardPreferences { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientPortalSession> ClientPortalSessions { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientPortalActivity> ClientPortalActivities { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SavedPayee> SavedPayees { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SavedTransferTemplate> SavedTransferTemplates { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientDocument> ClientDocuments { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientSupportTicket> ClientSupportTickets { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientSupportMessage> ClientSupportMessages { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientSupportAttachment> ClientSupportAttachments { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.FrequentlyAskedQuestion> FrequentlyAskedQuestions { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SavingsGoal> SavingsGoals { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SavingsGoalTransaction> SavingsGoalTransactions { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientNotification> ClientNotifications { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.NotificationDeliveryRecord> NotificationDeliveryRecords { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientSession> ClientSessions { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.ClientDevice> ClientDevices { get; set; }
        
        // Payments and Transfers
        public DbSet<FinTech.Domain.Entities.Banking.Biller> Billers { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.BillPayment> BillPayments { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.ExternalTransfer> ExternalTransfers { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.RecurringPayment> RecurringPayments { get; set; }
        public DbSet<FinTech.Domain.Entities.Banking.RecurringPaymentHistory> RecurringPaymentHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations from assembly
            builder.ApplyAllConfigurations();
            
            // Apply explicitly the new accounting configurations
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.ChartOfAccountConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.JournalEntryConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.JournalEntryLineConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.FinancialPeriodConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Configurations.Accounting.FiscalYearConfiguration());
            
            // Apply security and authorization configurations
            builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.ResourcePermissionConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.UserPermissionConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.SecurityPolicyConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.LoginAttemptConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Security.Authorization.DataAccessLogConfiguration());
            
            // Apply event tracking configurations
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Events.DomainEventRecordConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Messaging.OutboxMessageConfiguration());
            builder.ApplyConfiguration(new FinTech.Infrastructure.Data.Auditing.AuditLogConfiguration());
            
            // Apply global query filters
            ApplyGlobalFilters(builder);
        }
        
        private void ApplyGlobalFilters(ModelBuilder builder)
        {
            // Apply soft delete filter to all entities implementing ISoftDelete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(FinTech.Domain.Common.ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(FinTech.Domain.Common.ISoftDelete.IsDeleted));
                    var falseValue = Expression.Constant(false);
                    var expression = Expression.Equal(property, falseValue);
                    var lambda = Expression.Lambda(expression, parameter);
                    
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
                
                // Apply multi-tenancy filter if applicable
                if (typeof(FinTech.Domain.Common.ITenantEntity).IsAssignableFrom(entityType.ClrType) && _currentUserService != null)
                {
                    var tenantId = _currentUserService.TenantId;
                    if (tenantId.HasValue)
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var property = Expression.Property(parameter, nameof(FinTech.Domain.Common.ITenantEntity.TenantId));
                        var tenantValue = Expression.Constant(tenantId.Value);
                        var expression = Expression.Equal(property, tenantValue);
                        var lambda = Expression.Lambda(expression, parameter);
                        
                        builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Track entities being modified for auditing
            var auditEntries = OnBeforeSaveChanges();
            
            // Update audit fields
            foreach (var entry in ChangeTracker.Entries<FinTech.Domain.Common.BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.CreatedBy = _currentUserService.UserId.ToString();
                        }
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.UpdatedBy = _currentUserService.UserId.ToString();
                        }
                        break;
                    case EntityState.Deleted:
                        // Implement soft delete if entity implements ISoftDelete
                        if (entry.Entity is FinTech.Domain.Common.ISoftDelete softDeleteEntity)
                        {
                            entry.State = EntityState.Modified;
                            softDeleteEntity.IsDeleted = true;
                            softDeleteEntity.DeletedAt = DateTime.UtcNow;
                            if (_currentUserService != null)
                            {
                                softDeleteEntity.DeletedBy = _currentUserService.UserId.ToString();
                            }
                        }
                        break;
                }
            }

            // Get entities with domain events
            var entitiesWithEvents = ChangeTracker.Entries<FinTech.Domain.Common.BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();
                
            // Store domain events in the outbox for reliable processing
            AddDomainEventsToOutbox(entitiesWithEvents);

            // Save changes to database
            int result;
            try
            {
                result = await base.SaveChangesAsync(cancellationToken);
                
                // Write audit logs after successful save
                await OnAfterSaveChanges(auditEntries, cancellationToken);
                
                // Track database performance
                stopwatch.Stop();
                
                
                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    _logger?.LogWarning("SaveChangesAsync took {ElapsedMs}ms to complete with {EntitiesModified} entities modified",
                        stopwatch.ElapsedMilliseconds, result);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency conflict detected during SaveChanges");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during SaveChanges");
                throw;
            }

            // Then dispatch domain events
            await DispatchEvents(entitiesWithEvents, cancellationToken);

            return result;
        }

        private List<FinTech.Infrastructure.Data.Auditing.AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<FinTech.Infrastructure.Data.Auditing.AuditEntry>();
            
            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip entities that are not tracked, not changed, or not auditable
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (!(entry.Entity is FinTech.Domain.Common.IAuditable))
                    continue;

                var auditEntry = new FinTech.Infrastructure.Data.Auditing.AuditEntry
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = GetEntityId(entry),
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    UserId = _currentUserService?.UserId?.ToString() ?? "System",
                    TenantId = _currentUserService?.TenantId,
                    Changes = new Dictionary<string, object>()
                };

                foreach (var property in entry.Properties)
                {
                    // Skip navigation properties and collections
                    if (property.Metadata.IsKey() || property.Metadata.IsForeignKey() || property.GetCollectionAccessor() != null)
                        continue;

                    // Get property name
                    string propertyName = property.Metadata.Name;

                    // For created entities, log all property values
                    if (entry.State == EntityState.Added)
                    {
                        auditEntry.Changes[propertyName] = property.CurrentValue ?? DBNull.Value;
                    }
                    // For modified entities, log changed properties
                    else if (entry.State == EntityState.Modified && !property.IsModified)
                    {
                        // Skip unmodified properties
                        continue;
                    }
                    else if (entry.State == EntityState.Modified && property.IsModified)
                    {
                        auditEntry.Changes[propertyName] = new
                        {
                            OldValue = property.OriginalValue,
                            NewValue = property.CurrentValue
                        };
                    }
                    // For deleted entities, log all property values
                    else if (entry.State == EntityState.Deleted)
                    {
                        auditEntry.Changes[propertyName] = property.OriginalValue ?? DBNull.Value;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private async Task OnAfterSaveChanges(List<FinTech.Infrastructure.Data.Auditing.AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || !auditEntries.Any())
                return;
            
            // Add audit logs to the database
            foreach (var auditEntry in auditEntries)
            {
                var auditLog = new FinTech.Domain.Entities.Security.AuditLog
                {
                    EntityName = auditEntry.EntityName,
                    EntityId = auditEntry.EntityId,
                    Action = auditEntry.Action,
                    Timestamp = auditEntry.Timestamp,
                    UserId = auditEntry.UserId,
                    TenantId = auditEntry.TenantId,
                    Changes = JsonConvert.SerializeObject(auditEntry.Changes)
                };

                AuditLogs.Add(auditLog);
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        private string GetEntityId(EntityEntry entry)
        {
            var keyValues = entry.Metadata.FindPrimaryKey()
                .Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString());

            return string.Join(",", keyValues);
        }

        private void AddDomainEventsToOutbox(FinTech.Domain.Common.BaseEntity[] entitiesWithEvents)
        {
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToArray();
                
                foreach (var domainEvent in events)
                {
                    // Create an outbox message for each domain event
                    OutboxMessages.Add(new FinTech.Infrastructure.Data.Messaging.OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        EventType = domainEvent.GetType().AssemblyQualifiedName,
                        Content = JsonConvert.SerializeObject(domainEvent),
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = null,
                        Error = null
                    });
                    
                    // Create a domain event record for auditing
                    DomainEventRecords.Add(new FinTech.Infrastructure.Data.Events.DomainEventRecord
                    {
                        Id = Guid.NewGuid(),
                        EventType = domainEvent.GetType().Name,
                        EntityName = entity.GetType().Name,
                        EntityId = entity.Id.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        Data = JsonConvert.SerializeObject(domainEvent)
                    });
                }
                
                // Clear domain events after adding to outbox
                entity.ClearDomainEvents();
            }
        }

        private async Task DispatchEvents(FinTech.Domain.Common.BaseEntity[] entities, CancellationToken cancellationToken)
        {
            if (_domainEventService == null)
            {
                _logger?.LogWarning("Domain event service is not available. Events will not be dispatched.");
                return;
            }

            foreach (var entity in entities)
            {
                var events = entity.DomainEvents.ToArray();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    try
                    {
                        var eventName = domainEvent.GetType().Name;
                        var entityType = entity.GetType().Name;
                        
                        _logger?.LogInformation("Dispatching domain event {EventName} for entity {EntityType} with ID {EntityId}",
                            eventName, entityType, entity.Id);
                            
                        var stopwatch = Stopwatch.StartNew();
                        
                        await _domainEventService.PublishAsync(domainEvent, cancellationToken);
                        
                        stopwatch.Stop();
                        
                        
                        _logger?.LogInformation("Successfully dispatched domain event {EventName} for entity {EntityType} with ID {EntityId} in {ElapsedMs}ms",
                            eventName, entityType, entity.Id, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error publishing domain event {EventType} for entity {EntityType} with ID {EntityId}",
                            domainEvent.GetType().Name, entity.GetType().Name, entity.Id);
                            
                        
                        // We don't want to throw here to avoid transaction rollback
                        // Instead, log the error and continue
                        
                        // Update the outbox message with error information
                        var outboxMessage = await OutboxMessages
                            .FirstOrDefaultAsync(m => 
                                m.EventType == domainEvent.GetType().AssemblyQualifiedName && 
                                m.ProcessedAt == null);
                                
                        if (outboxMessage != null)
                        {
                            outboxMessage.Error = ex.ToString();
                            outboxMessage.ProcessedAt = DateTime.UtcNow;
                            await base.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
