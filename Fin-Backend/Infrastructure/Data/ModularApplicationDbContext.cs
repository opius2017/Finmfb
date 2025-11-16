using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

using FinTech.Infrastructure.Data.Contexts.Features;
using FinTech.Core.Application.Interfaces;
using FinTech.Core.Domain.Common;
using FinTech.Infrastructure.Services;

namespace FinTech.Infrastructure.Data
{
    /// <summary>
    /// Modular ApplicationDbContext that coordinates feature-specific contexts
    /// </summary>
    public class ModularApplicationDbContext : IdentityDbContext<FinTech.Core.Domain.Entities.Identity.ApplicationUser>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDomainEventService _domainEventService;
        private readonly ILogger<ModularApplicationDbContext> _logger;

        public ModularApplicationDbContext(
            DbContextOptions<ModularApplicationDbContext> options,
            ICurrentUserService currentUserService = null,
            IDomainEventService domainEventService = null,
            ILogger<ModularApplicationDbContext> logger = null) : base(options)
        {
            _currentUserService = currentUserService;
            _domainEventService = domainEventService;
            _logger = logger;
        }

        // Feature-specific contexts are composed here
        // We use the same connection string but separate logical boundaries

        #region Core System Entities
        // Domain Events and Messaging
        public DbSet<FinTech.Infrastructure.Data.Events.DomainEventRecord> DomainEventRecords { get; set; }
        public DbSet<FinTech.Infrastructure.Data.Messaging.OutboxMessage> OutboxMessages { get; set; }
        
        // Auditing and Security
        public DbSet<FinTech.Core.Domain.Entities.Security.AuditLog> AuditLogs { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Security.LoginAttempt> LoginAttempts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Security.DataAccessLog> DataAccessLogs { get; set; }
        
        // Multi-tenancy and Authorization
        public DbSet<FinTech.Core.Domain.Entities.Security.ResourcePermission> ResourcePermissions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Security.UserPermission> UserPermissions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Security.SecurityPolicy> SecurityPolicies { get; set; }
        #endregion

        #region Feature Module Entities
        // Each feature module's entities are defined here for now
        // In a true microservices architecture, these would be in separate databases
        
        // Loans Feature Module
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanProduct> LoanProducts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanApplication> LoanApplications { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanGuarantor> LoanGuarantors { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanCollateral> LoanCollaterals { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanCollateralDocument> LoanCollateralDocuments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanDocument> LoanDocuments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.Loan> Loans { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanAccount> LoanAccounts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanTransaction> LoanTransactions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Loans.LoanClassificationHistory> LoanClassificationHistories { get; set; }

        // Accounting Feature Module
        public DbSet<FinTech.Core.Domain.Entities.Accounting.ChartOfAccount> ChartOfAccounts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.JournalEntry> JournalEntries { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.TrialBalance> TrialBalances { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.FinancialPeriod> FinancialPeriods { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.FiscalYear> FiscalYears { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.AccountsPayable> AccountsPayables { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.AccountsReceivable> AccountsReceivables { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.Vendor> Vendors { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.VendorCategory> VendorCategories { get; set; }

        // Customer Management Feature Module
        public DbSet<FinTech.Core.Domain.Entities.CustomerManagement.Customer> Customers { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.CustomerManagement.CustomerDocument> CustomerDocuments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.CustomerManagement.CustomerInquiry> CustomerInquiries { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.CustomerManagement.CustomerComplaint> CustomerComplaints { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.CustomerManagement.CustomerCommunication> CustomerCommunications { get; set; }

        // Deposits Feature Module
        public DbSet<FinTech.Core.Domain.Entities.Deposits.DepositProduct> DepositProducts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Deposits.DepositAccount> DepositAccounts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Deposits.DepositTransaction> DepositTransactions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Deposits.InterestCalculation> InterestCalculations { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Deposits.DepositSweepRule> DepositSweepRules { get; set; }
        
        // Client Portal Feature Module
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientPortalProfile> ClientPortalProfiles { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.NotificationPreferences> NotificationPreferences { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.DashboardPreferences> DashboardPreferences { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientPortalSession> ClientPortalSessions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientPortalActivity> ClientPortalActivities { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.SavedPayee> SavedPayees { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.SavedTransferTemplate> SavedTransferTemplates { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientDocument> ClientDocuments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientSupportTicket> ClientSupportTickets { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientSupportMessage> ClientSupportMessages { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientSupportAttachment> ClientSupportAttachments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.SavingsGoal> SavingsGoals { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.SavingsGoalTransaction> SavingsGoalTransactions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientNotification> ClientNotifications { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.NotificationDeliveryRecord> NotificationDeliveryRecords { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientSession> ClientSessions { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.ClientPortal.ClientDevice> ClientDevices { get; set; }
        
        // Payments Feature Module
        public DbSet<FinTech.Core.Domain.Entities.Payments.Biller> Billers { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Payments.BillPayment> BillPayments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Payments.ExternalTransfer> ExternalTransfers { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Payments.RecurringPayment> RecurringPayments { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Payments.RecurringPaymentHistory> RecurringPaymentHistory { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations by feature module
            ApplyFeatureConfigurations(builder);
            
            // Apply global query filters
            ApplyGlobalFilters(builder);
        }

        private void ApplyFeatureConfigurations(ModelBuilder builder)
        {
            // Apply configurations organized by feature module
            var assembly = typeof(ModularApplicationDbContext).Assembly;
            
            // Loans feature configurations
            builder.ApplyConfigurationsFromAssembly(assembly, 
                type => type.Namespace?.Contains("Loans") == true);
                
            // Accounting feature configurations  
            builder.ApplyConfigurationsFromAssembly(assembly,
                type => type.Namespace?.Contains("Accounting") == true);
                
            // Customer Management feature configurations
            builder.ApplyConfigurationsFromAssembly(assembly,
                type => type.Namespace?.Contains("CustomerManagement") == true);
                
            // Deposits feature configurations
            builder.ApplyConfigurationsFromAssembly(assembly,
                type => type.Namespace?.Contains("Deposits") == true);
                
            // Client Portal feature configurations
            builder.ApplyConfigurationsFromAssembly(assembly,
                type => type.Namespace?.Contains("ClientPortal") == true);
                
            // Payments feature configurations
            builder.ApplyConfigurationsFromAssembly(assembly,
                type => type.Namespace?.Contains("Payments") == true);
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
                    var tenantId = _currentUserService.TenantId;
                    if (tenantId.HasValue)
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var property = Expression.Property(parameter, nameof(FinTech.Core.Domain.Common.ITenantEntity.TenantId));
                        var tenantValue = Expression.Constant(tenantId.Value);
                        var expression = Expression.Equal(property, tenantValue);
                        var lambda = Expression.Lambda(expression, parameter);
                        
                        builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }

        // Simplified SaveChangesAsync for the new structure
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Update audit fields
            UpdateAuditFields();
            
            // Handle soft deletes
            HandleSoftDeletes();

            // Get entities with domain events before saving
            var entitiesWithEvents = GetEntitiesWithEvents();
            
            // Store domain events in outbox for reliable processing
            AddDomainEventsToOutbox(entitiesWithEvents);

            int result;
            try
            {
                result = await base.SaveChangesAsync(cancellationToken);
                stopwatch.Stop();
                
                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    _logger?.LogWarning("SaveChangesAsync took {ElapsedMs}ms to complete with {EntitiesModified} entities modified",
                        stopwatch.ElapsedMilliseconds, result);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during SaveChanges");
                throw;
            }

            // Dispatch domain events after successful save
            await DispatchEvents(entitiesWithEvents, cancellationToken);

            return result;
        }

        private void UpdateAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries<FinTech.Core.Domain.Entities.Common.BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.CreatedBy = _currentUserService.UserId.ToString();
                        }
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.UtcNow;
                        if (_currentUserService != null)
                        {
                            entry.Entity.LastModifiedBy = _currentUserService.UserId.ToString();
                        }
                        break;
                }
            }
        }

        private void HandleSoftDeletes()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Deleted && entry.Entity is FinTech.Core.Domain.Common.ISoftDelete softDeleteEntity)
                {
                    entry.State = EntityState.Modified;
                    softDeleteEntity.IsDeleted = true;
                    softDeleteEntity.DeletedAt = DateTime.UtcNow;
                    if (_currentUserService != null)
                    {
                        softDeleteEntity.DeletedBy = _currentUserService.UserId.ToString();
                    }
                }
            }
        }

        private FinTech.Core.Domain.Entities.Common.AggregateRoot[] GetEntitiesWithEvents()
        {
            return ChangeTracker.Entries<FinTech.Core.Domain.Entities.Common.AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();
        }

        private void AddDomainEventsToOutbox(FinTech.Core.Domain.Entities.Common.AggregateRoot[] entitiesWithEvents)
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

        private async Task DispatchEvents(FinTech.Core.Domain.Entities.Common.AggregateRoot[] entities, CancellationToken cancellationToken)
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
                        await _domainEventService.PublishAsync(domainEvent, cancellationToken);
                        _logger?.LogInformation("Successfully dispatched domain event {EventName} for entity {EntityType}",
                            domainEvent.GetType().Name, entity.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error publishing domain event {EventType} for entity {EntityType}",
                            domainEvent.GetType().Name, entity.GetType().Name);
                    }
                }
            }
        }
    }
}