using System;
using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a financial year for accounting purposes
    /// </summary>
    public class FiscalYear : AggregateRoot
    {
        public int Year { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsClosed { get; private set; }
        public DateTime? ClosedDate { get; private set; }
        public string ClosedBy { get; private set; }
        public bool IsCurrentYear { get; private set; }
        
        private List<FinancialPeriod> _periods = new List<FinancialPeriod>();
        public IReadOnlyCollection<FinancialPeriod> Periods => _periods.AsReadOnly();
        
        // Required by EF Core
        private FiscalYear() { }
        
        public FiscalYear(
            int year,
            DateTime startDate,
            DateTime endDate,
            bool isCurrentYear = false)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");
                
            Year = year;
            StartDate = startDate;
            EndDate = endDate;
            IsClosed = false;
            IsCurrentYear = isCurrentYear;
            
            AddDomainEvent(new FiscalYearCreatedEvent(Id, year, startDate, endDate));
        }
        
        public void AddPeriod(FinancialPeriod period)
        {
            if (period.StartDate < StartDate || period.EndDate > EndDate)
                throw new ArgumentException("Period dates must be within fiscal year dates");
                
            _periods.Add(period);
        }
        
        public void CloseYear(string closedBy)
        {
            if (IsClosed)
                throw new InvalidOperationException($"Fiscal year {Year} is already closed");
                
            // Check if all periods are closed
            foreach (var period in _periods)
            {
                if (!period.IsClosed)
                    throw new InvalidOperationException($"Cannot close fiscal year when period {period.PeriodCode} is still open");
            }
            
            IsClosed = true;
            ClosedDate = DateTime.UtcNow;
            ClosedBy = closedBy;
            IsCurrentYear = false;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FiscalYearClosedEvent(Id, Year, closedBy));
        }
        
        public void SetAsCurrent()
        {
            if (IsClosed)
                throw new InvalidOperationException($"Cannot set closed fiscal year {Year} as current");
                
            IsCurrentYear = true;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new FiscalYearSetAsCurrentEvent(Id, Year));
        }
    }
    
    // Domain Events
    public class FiscalYearCreatedEvent : DomainEvent
    {
        public string FiscalYearId { get; }
        public int Year { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        
        public FiscalYearCreatedEvent(
            string fiscalYearId, 
            int year, 
            DateTime startDate, 
            DateTime endDate)
        {
            FiscalYearId = fiscalYearId;
            Year = year;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
    
    public class FiscalYearClosedEvent : DomainEvent
    {
        public string FiscalYearId { get; }
        public int Year { get; }
        public string ClosedBy { get; }
        
        public FiscalYearClosedEvent(string fiscalYearId, int year, string closedBy)
        {
            FiscalYearId = fiscalYearId;
            Year = year;
            ClosedBy = closedBy;
        }
    }
    
    public class FiscalYearSetAsCurrentEvent : DomainEvent
    {
        public string FiscalYearId { get; }
        public int Year { get; }
        
        public FiscalYearSetAsCurrentEvent(string fiscalYearId, int year)
        {
            FiscalYearId = fiscalYearId;
            Year = year;
        }
    }
}