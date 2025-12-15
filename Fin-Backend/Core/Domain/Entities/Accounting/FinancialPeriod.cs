using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents the status of the period closing process
    /// </summary>
    public enum ClosingStatus
    {
        NotStarted,
        Initiated,
        ValidationFailed,
        Validated,
        ClosingEntriesPosted,
        Completed,
        Failed
    }

    public enum FinancialPeriodStatus
    {
        Undefined,
        Planned,
        Open,
        Closed,
        Archived
    }

    /// <summary>
    /// Represents a financial period for accounting purposes
    /// </summary>
    public class FinancialPeriod : AggregateRoot
    {
        public string PeriodCode { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty; // Renamed from PeriodName for consistency
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string? ClosedBy { get; set; }
        public int FiscalYear { get; private set; }
        public int FiscalMonth { get; private set; }
        public bool IsAdjustmentPeriod { get; private set; }
        
        // Missing property fixes
        public string? FiscalYearId { get; set; }
        public FinancialPeriodStatus Status { get; set; }
        public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        public DateTime? LastModifiedAt { get => LastModifiedDate; set => LastModifiedDate = value; }
        
        // New properties for period closing process
        public ClosingStatus ClosingStatus { get; private set; } = ClosingStatus.NotStarted;
        public string? ValidationErrors { get; private set; }
        public DateTime? ClosingStartedAt { get; private set; }
        public DateTime? ClosingCompletedAt { get; private set; }
        public string? ClosingInitiatedBy { get; private set; }
        
        // Required by EF Core
        private FinancialPeriod() { }
        
        public FinancialPeriod(
            string periodCode,
            string periodName,
            DateTime startDate,
            DateTime endDate,
            int fiscalYear,
            int fiscalMonth,
            bool isAdjustmentPeriod = false)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
                
            PeriodCode = periodCode;
            Name = periodName;
            StartDate = startDate;
            EndDate = endDate;
            IsClosed = false;
            FiscalYear = fiscalYear;
            FiscalMonth = fiscalMonth;
            IsAdjustmentPeriod = isAdjustmentPeriod;
            ClosingStatus = ClosingStatus.NotStarted;
            
            AddDomainEvent(new FinancialPeriodCreatedEvent(Id, periodCode, periodName, startDate, endDate));
        }
        
        /// <summary>
        /// Starts the closing process for this financial period
        /// </summary>
        public void StartClosingProcess(string initiatedBy = "System")
        {
            if (IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is already closed");
                
            if (ClosingStatus != ClosingStatus.NotStarted && ClosingStatus != ClosingStatus.Failed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is already in the process of closing");
                
            ClosingStatus = ClosingStatus.Initiated;
            ClosingStartedAt = DateTime.UtcNow;
            ClosingInitiatedBy = initiatedBy;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new PeriodClosingInitiatedEvent(Id, PeriodCode, initiatedBy));
        }
        
        /// <summary>
        /// Sets validation errors for the closing process
        /// </summary>
        public void SetValidationErrors(string errors)
        {
            if (ClosingStatus != ClosingStatus.Initiated)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not in the Initiated state");
                
            ValidationErrors = errors;
            ClosingStatus = ClosingStatus.ValidationFailed;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new PeriodClosingValidationFailedEvent(Id, PeriodCode, errors));
        }
        
        /// <summary>
        /// Completes the validation phase of the closing process
        /// </summary>
        public void CompleteValidation()
        {
            if (ClosingStatus != ClosingStatus.Initiated)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not in the Initiated state");
                
            ValidationErrors = null;
            ClosingStatus = ClosingStatus.Validated;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new PeriodClosingValidatedEvent(Id, PeriodCode));
        }
        
        /// <summary>
        /// Completes the posting of closing entries
        /// </summary>
        public void CompleteClosingEntries()
        {
            if (ClosingStatus != ClosingStatus.Validated)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not in the Validated state");
                
            ClosingStatus = ClosingStatus.ClosingEntriesPosted;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new PeriodClosingEntriesPostedEvent(Id, PeriodCode));
        }
        
        /// <summary>
        /// Completes the closing process and marks the period as closed
        /// </summary>
        public void Close(string closedBy = "System")
        {
            if (IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is already closed");
                
            if (ClosingStatus != ClosingStatus.ClosingEntriesPosted)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not ready to be closed");
                
            IsClosed = true;
            ClosedDate = DateTime.UtcNow;
            ClosedBy = closedBy;
            ClosingStatus = ClosingStatus.Completed;
            ClosingCompletedAt = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FinancialPeriodClosedEvent(Id, PeriodCode, closedBy));
        }
        
        /// <summary>
        /// Rolls back the closing process
        /// </summary>
        public void RollBackClosingProcess(string reason)
        {
            if (IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is already closed and cannot be rolled back");
                
            if (ClosingStatus == ClosingStatus.NotStarted)
                throw new InvalidOperationException($"Financial period {PeriodCode} has not started the closing process");
                
            ClosingStatus = ClosingStatus.Failed;
            ValidationErrors = reason;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new PeriodClosingRolledBackEvent(Id, PeriodCode, reason));
        }
        
        /// <summary>
        /// Reopens a closed financial period (use with caution)
        /// </summary>
        public void ReopenPeriod(string reopenedBy, string reason)
        {
            if (!IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not closed");
                
            IsClosed = false;
            ClosedDate = null;
            ClosedBy = null;
            ClosingStatus = ClosingStatus.NotStarted;
            ClosingStartedAt = null;
            ClosingCompletedAt = null;
            ClosingInitiatedBy = null;
            ValidationErrors = null;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FinancialPeriodReopenedEvent(Id, PeriodCode, reopenedBy, reason));
        }
    }
    
    // Domain Events
    public class FinancialPeriodCreatedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string PeriodName { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        
        public FinancialPeriodCreatedEvent(
            string periodId, 
            string periodCode, 
            string periodName, 
            DateTime startDate, 
            DateTime endDate)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            PeriodName = periodName;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
    
    public class FinancialPeriodClosedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string ClosedBy { get; }
        
        public FinancialPeriodClosedEvent(string periodId, string periodCode, string closedBy)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            ClosedBy = closedBy;
        }
    }
    
    public class FinancialPeriodReopenedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string ReopenedBy { get; }
        public string Reason { get; }
        
        public FinancialPeriodReopenedEvent(string periodId, string periodCode, string reopenedBy, string reason)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            ReopenedBy = reopenedBy;
            Reason = reason;
        }
    }
    
    public class PeriodClosingInitiatedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string InitiatedBy { get; }
        
        public PeriodClosingInitiatedEvent(string periodId, string periodCode, string initiatedBy)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            InitiatedBy = initiatedBy;
        }
    }
    
    public class PeriodClosingValidationFailedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string ValidationErrors { get; }
        
        public PeriodClosingValidationFailedEvent(string periodId, string periodCode, string validationErrors)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            ValidationErrors = validationErrors;
        }
    }
    
    public class PeriodClosingValidatedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        
        public PeriodClosingValidatedEvent(string periodId, string periodCode)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
        }
    }
    
    public class PeriodClosingEntriesPostedEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        
        public PeriodClosingEntriesPostedEvent(string periodId, string periodCode)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
        }
    }
    
    public class PeriodClosingRolledBackEvent : DomainEvent
    {
        public string PeriodId { get; }
        public string PeriodCode { get; }
        public string Reason { get; }
        
        public PeriodClosingRolledBackEvent(string periodId, string periodCode, string reason)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            Reason = reason;
        }
    }
}
