using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
using FinTech.Domain.Entities.Accounting;
using FinTech.Infrastructure.Data.Configuration;
using FinTech.Infrastructure.Data.Configurations.Accounting;
using FinTech.Infrastructure.Data.Interceptors;
using FinTech.Infrastructure.Security.Authorization;
using FinTech.WebAPI.Domain.Entities.Auth;
using Microsoft.ApplicationInsights;
using FinTech.Infrastructure.Data.Auditing;
using FinTech.Infrastructure.Data.Events;
using FinTech.Infrastructure.Data.Messaging;
using FinTech.Infrastructure.Messaging;

namespace FinTech.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, FinTech.Core.Application.Common.Interfaces.IApplicationDbContext
    {
        private readonly IDomainEventService _domainEventService;
        private readonly ILogger<ApplicationDbContext> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IDomainEventService domainEventService = null,
            ILogger<ApplicationDbContext> logger = null,
            TelemetryClient telemetryClient = null,
            ICurrentUserService currentUserService = null) 
            : base(options) 
        {
            _domainEventService = domainEventService;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _currentUserService = currentUserService;
        }

        // General Ledger - Legacy
        public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<FinTech.Domain.Entities.GeneralLedger.JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryDetail> JournalEntryDetails { get; set; }
        
        // Accounting - New Core Accounting Engine
        public DbSet<ChartOfAccount> CoreChartOfAccounts { get; set; }
        public DbSet<FinTech.Domain.Entities.Accounting.JournalEntry> CoreJournalEntries { get; set; }
        public DbSet<JournalEntryLine> CoreJournalEntryLines { get; set; }
        public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }

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
        public DbSet<FinTech.Domain.Entities.Security.AuditLog> AuditLogs { get; set; }
        public DbSet<MakerCheckerTransaction> MakerCheckerTransactions { get; set; }
        public DbSet<ResourcePermission> ResourcePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<ResourceOperation> ResourceOperations { get; set; }
        public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<DataAccessLog> DataAccessLogs { get; set; }
        
        // Domain Events Tracking
        public DbSet<FinTech.Infrastructure.Data.Events.DomainEventRecord> DomainEventRecords { get; set; }
        public DbSet<FinTech.Infrastructure.Data.Messaging.OutboxMessage> OutboxMessages { get; set; }
        
        // Integration Events Tracking
        public DbSet<IntegrationEventOutboxItem> IntegrationEventOutbox { get; set; }

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
        public DbSet<FinTech.Domain.Entities.ClientPortal.SecurityActivity> SecurityActivities { get; set; }
        public DbSet<FinTech.Domain.Entities.ClientPortal.SecurityPreferences> SecurityPreferences { get; set; }

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
            
            // Apply explicitly the new accounting configurations
            builder.ApplyConfiguration(new ChartOfAccountConfiguration());
            builder.ApplyConfiguration(new JournalEntryConfiguration());
            builder.ApplyConfiguration(new JournalEntryLineConfiguration());
            builder.ApplyConfiguration(new FinancialPeriodConfiguration());
            builder.ApplyConfiguration(new FiscalYearConfiguration());
            
            // Apply security and authorization configurations
            builder.ApplyConfiguration(new ResourcePermissionConfiguration());
            builder.ApplyConfiguration(new UserPermissionConfiguration());
            builder.ApplyConfiguration(new SecurityPolicyConfiguration());
            builder.ApplyConfiguration(new LoginAttemptConfiguration());
            builder.ApplyConfiguration(new DataAccessLogConfiguration());
            
            // Apply event tracking configurations
            builder.ApplyConfiguration(new DomainEventRecordConfiguration());
            builder.ApplyConfiguration(new OutboxMessageConfiguration());
            builder.ApplyConfiguration(new AuditLogConfiguration());
            
            // Apply global query filters
            ApplyGlobalFilters(builder);
        }
        
        private void ApplyGlobalFilters(ModelBuilder builder)
        {
            // Apply soft delete filter to all entities implementing ISoftDelete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var falseValue = Expression.Constant(false);
                    var expression = Expression.Equal(property, falseValue);
                    var lambda = Expression.Lambda(expression, parameter);
                    
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
                
                // Apply multi-tenancy filter if applicable
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType) && _currentUserService != null)
                {
                    var tenantId = _currentUserService.TenantId;
                    if (tenantId.HasValue)
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
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
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.CreatedBy = _currentUserService.UserId;
                        }
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.UpdatedBy = _currentUserService.UserId;
                        }
                        break;
                    case EntityState.Deleted:
                        // Implement soft delete if entity implements ISoftDelete
                        if (entry.Entity is ISoftDelete softDeleteEntity)
                        {
                            entry.State = EntityState.Modified;
                            softDeleteEntity.IsDeleted = true;
                            softDeleteEntity.DeletedAt = DateTime.UtcNow;
                            if (_currentUserService != null)
                            {
                                softDeleteEntity.DeletedBy = _currentUserService.UserId;
                            }
                        }
                        break;
                }
            }

            // Get entities with domain events
            var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
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
                
                if (_telemetryClient != null)
                {
                    _telemetryClient.TrackMetric("Database.SaveChanges.Duration", stopwatch.ElapsedMilliseconds);
                    _telemetryClient.TrackMetric("Database.SaveChanges.EntitiesModified", result);
                }
                
                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    _logger?.LogWarning("SaveChangesAsync took {ElapsedMs}ms to complete with {EntitiesModified} entities modified",
                        stopwatch.ElapsedMilliseconds, result);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency conflict detected during SaveChanges");
                if (_telemetryClient != null)
                {
                    _telemetryClient.TrackException(ex);
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during SaveChanges");
                if (_telemetryClient != null)
                {
                    _telemetryClient.TrackException(ex);
                }
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

                if (!(entry.Entity is IAuditable))
                    continue;

                var auditEntry = new FinTech.Infrastructure.Data.Auditing.AuditEntry
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = GetEntityId(entry),
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    UserId = _currentUserService?.UserId,
                    TenantId = _currentUserService?.TenantId,
                    Changes = new Dictionary<string, object>()
                };

                foreach (var property in entry.Properties)
                {
                    // Skip navigation properties and collections
                    if (property.Metadata.IsKey() || property.Metadata.IsForeignKey() || property.Metadata.IsCollection())
                        continue;

                    // Get property name
                    string propertyName = property.Metadata.Name;

                    // For created entities, log all property values
                    if (entry.State == EntityState.Added)
                    {
                        auditEntry.Changes[propertyName] = property.CurrentValue;
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
                        auditEntry.Changes[propertyName] = property.OriginalValue;
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
                var auditLog = new AuditLog
                {
                    EntityName = auditEntry.EntityName,
                    EntityId = auditEntry.EntityId,
                    Action = auditEntry.Action,
                    Timestamp = auditEntry.Timestamp,
                    UserId = auditEntry.UserId,
                    TenantId = auditEntry.TenantId,
                    Changes = Newtonsoft.Json.JsonConvert.SerializeObject(auditEntry.Changes)
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

        private void AddDomainEventsToOutbox(BaseEntity[] entitiesWithEvents)
        {
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToArray();
                
                foreach (var domainEvent in events)
                {
                    // Create an outbox message for each domain event
                    OutboxMessages.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        EventType = domainEvent.GetType().AssemblyQualifiedName,
                        Content = Newtonsoft.Json.JsonConvert.SerializeObject(domainEvent),
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = null,
                        Error = null
                    });
                    
                    // Create a domain event record for auditing
                    DomainEventRecords.Add(new DomainEventRecord
                    {
                        Id = Guid.NewGuid(),
                        EventType = domainEvent.GetType().Name,
                        EntityName = entity.GetType().Name,
                        EntityId = entity.Id.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(domainEvent)
                    });
                }
                
                // Clear domain events after adding to outbox
                entity.ClearDomainEvents();
            }
        }

        private async Task DispatchEvents(BaseEntity[] entities, CancellationToken cancellationToken)
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
                        
                        // Track event processing performance
                        if (_telemetryClient != null)
                        {
                            _telemetryClient.TrackMetric($"DomainEvent.{eventName}.ProcessingTime", stopwatch.ElapsedMilliseconds);
                            _telemetryClient.TrackEvent($"DomainEvent.{eventName}.Processed", new Dictionary<string, string>
                            {
                                ["EntityType"] = entityType,
                                ["EntityId"] = entity.Id.ToString()
                            });
                        }
                        
                        _logger?.LogInformation("Successfully dispatched domain event {EventName} for entity {EntityType} with ID {EntityId} in {ElapsedMs}ms",
                            eventName, entityType, entity.Id, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error publishing domain event {EventType} for entity {EntityType} with ID {EntityId}",
                            domainEvent.GetType().Name, entity.GetType().Name, entity.Id);
                            
                        if (_telemetryClient != null)
                        {
                            _telemetryClient.TrackException(ex, new Dictionary<string, string>
                            {
                                ["EventType"] = domainEvent.GetType().Name,
                                ["EntityType"] = entity.GetType().Name,
                                ["EntityId"] = entity.Id.ToString()
                            });
                        }
                        
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
