using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a financial period for accounting purposes
    /// </summary>
    public class FinancialPeriod : AggregateRoot
    {
        public string PeriodCode { get; private set; }
        public string PeriodName { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsClosed { get; private set; }
        public DateTime? ClosedDate { get; private set; }
        public string ClosedBy { get; private set; }
        public int FiscalYear { get; private set; }
        public int FiscalMonth { get; private set; }
        public bool IsAdjustmentPeriod { get; private set; }
        
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
            PeriodName = periodName;
            StartDate = startDate;
            EndDate = endDate;
            IsClosed = false;
            FiscalYear = fiscalYear;
            FiscalMonth = fiscalMonth;
            IsAdjustmentPeriod = isAdjustmentPeriod;
            
            AddDomainEvent(new FinancialPeriodCreatedEvent(Id, periodCode, periodName, startDate, endDate));
        }
        
        public void ClosePeriod(string closedBy)
        {
            if (IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is already closed");
                
            IsClosed = true;
            ClosedDate = DateTime.UtcNow;
            ClosedBy = closedBy;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FinancialPeriodClosedEvent(Id, PeriodCode, closedBy));
        }
        
        public void ReopenPeriod(string reopenedBy)
        {
            if (!IsClosed)
                throw new InvalidOperationException($"Financial period {PeriodCode} is not closed");
                
            IsClosed = false;
            ClosedDate = null;
            ClosedBy = null;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FinancialPeriodReopenedEvent(Id, PeriodCode, reopenedBy));
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
        
        public FinancialPeriodReopenedEvent(string periodId, string periodCode, string reopenedBy)
        {
            PeriodId = periodId;
            PeriodCode = periodCode;
            ReopenedBy = reopenedBy;
        }
    }
}